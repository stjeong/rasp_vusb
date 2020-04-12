
#include "stdafx.h"

#include <libgen.h>
#include "VUsbServer.h"

VUsbServer::VUsbServer()
{
    _sock = 0;
    _sslContext = nullptr;

    get_module_dir_path(_modulePath);

    init_openssl();
    _emulator.Run();
}

VUsbServer::~VUsbServer()
{
    dispose();
    cleanup_openssl();
}

void VUsbServer::get_module_dir_path(char modulePath[])
{
    char arg1[] = "/proc/self/exe";
    char exepath[PATH_MAX + 1] = { 0 };

    readlink(arg1, exepath, PATH_MAX);
    strcpy(modulePath, dirname(exepath));

    printf("module path: %s\n", modulePath);
}

void VUsbServer::dispose()
{
    if (_sock != 0)
    {
        ::close(_sock);
        _sock = 0;
    }

    _emulator.Close();
}

bool VUsbServer::open()
{
    _emulator.Open();

    _sock = socket(AF_INET, SOCK_STREAM, 0);
    if (_sock < 0) {
        perror("VUsbServer::open - sock error\n");
        return false;
    }

    int addr_len = sizeof(struct sockaddr_in);

    memset((void*)&_server_addr, 0, addr_len);
    _server_addr.sin_family = AF_INET;
    _server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
    _server_addr.sin_port = htons((PORT + 1));

    int ret = ::bind(_sock, (struct sockaddr*) & _server_addr, addr_len);
    if (ret < 0) {
        perror("VUsbServer::open - bind error\n");
        return false;
    }

    ::listen(_sock, 5);

    return true;
}

bool VUsbServer::ReadData(SSL* ssl, char* buf, int len)
{
    int totalRead = len;

    while (totalRead > 0)
    {
        // int readBytes = recv(socket, buf, len, 0);
        int readBytes = SSL_read(ssl, (char*)buf, len);

        if (readBytes <= 0)
        {
            break;
        }

        totalRead -= readBytes;
    }

    if (totalRead == 0)
    {
        return true;
    }

    return false;
}

void VUsbServer::startService()
{
    thread t([&]()
        {
            struct sockaddr_in server_socket;
            socklen_t addr_len = sizeof(server_socket);

            while (true)
            {
                int clntSocket = accept(_sock, (struct sockaddr*) & server_socket, &addr_len);

                SSL* ssl = nullptr;
                ssl = SSL_new(_sslContext);
                SSL_set_fd(ssl, clntSocket);
                int ssl_err = SSL_accept(ssl);
                if (ssl_err <= 0)
                {
                    break;
                }

                thread clntThread([this](int connectedSocket, UsbEmulator& emulator, SSL* pSsl)
                    {
                        char buf[4096];

                        while (true)
                        {
                            memset(buf, 0, sizeof(buf));

                            // get length of packet
                            bool result = ReadData(pSsl, buf, 4);
                            if (result == false)
                            {
                                break;
                            }

                            int packetLen = *(int*)buf;

                            if (packetLen > 4096)
                            {
                                ReadData(pSsl, buf, packetLen);
                                printf("dropped long packet = size(%d)\n", packetLen);
                                break;
                            }

                            result = ReadData(pSsl, buf, packetLen);
                            if (result == false)
                            {
                                printf("can't read = len(%d)\n", packetLen);
                                break;
                            }

                            printf("recvBytes == %d\n", packetLen);

                            if (packetLen <= 0)
                            {
                                break;
                            }

                            if (emulator.Enqueue(buf, packetLen) == false)
                            {
                                SendAck(pSsl);
                                break;
                            }
                        }

                        if (pSsl != nullptr)
                        {
                            printf("SSL_free\n");
                            SSL_free(pSsl);
                            printf("completed: SSL_free\n");
                        }

                        if (connectedSocket > 0)
                        {
                            ::close(connectedSocket);
                        }

                    }, clntSocket, std::ref(_emulator), ssl);

                clntThread.detach();
            }

        });

    t.detach();
}

void VUsbServer::SendAck(SSL* pSsl)
{
    char buf[] = { ACK_RESPONSE_CMD };
    SSL_write(pSsl, buf, 1);
}

void VUsbServer::init_openssl()
{
    cout << "init_openssl" << endl;

    SSL_load_error_strings();
    SSL_library_init();
    OpenSSL_add_all_algorithms();

    _sslContext = SSL_CTX_new(SSLv23_server_method());

#ifdef _DEBUG
#else
    char certPath[PATH_MAX];
    char keyPath[PATH_MAX];

    sprintf(certPath, "%s/%s", _modulePath, "test.pem");
    sprintf(keyPath, "%s/%s", _modulePath, "key.pem");
#endif

    if (_sslContext == nullptr)
    {
        return;
    }

    SSL_CTX_set_options(_sslContext, SSL_OP_SINGLE_DH_USE);
    bool initialized = true;
    int sslResult = 0;

    do
    {
        sslResult = SSL_CTX_use_certificate_file(_sslContext,
#ifdef _DEBUG
            "/share/test.pem"
#else
            certPath
#endif
            , SSL_FILETYPE_PEM);

        if (sslResult <= 0)
        {
            cout << "failed: cert file not found - " << ERR_error_string(ERR_get_error(), nullptr) << endl;
            initialized = false;
            break;
        }

        if (SSL_CTX_use_PrivateKey_file(_sslContext,
#ifdef _DEBUG
            "/share/key.pem"
#else
            keyPath
#endif
            , SSL_FILETYPE_PEM) <= 0)
        {
            cout << "failed: private key file not found" << endl;
            initialized = false;
            break;
        }

        cout << "completed: init_openssl" << endl;

    } while (false);

    if (initialized == false)
    {
        cout << "failed: init_openssl" << endl;
        free_sslctx();
    }
}

void VUsbServer::free_sslctx()
{
    if (_sslContext != nullptr)
    {
        SSL_CTX_free(_sslContext);
        _sslContext = nullptr;
    }
}

void VUsbServer::cleanup_openssl()
{
    free_sslctx();

    ERR_free_strings();
    EVP_cleanup();
}
