#pragma once

#include <condition_variable>
#include <atomic>
#include <tuple>

// https://codereview.stackexchange.com/questions/75606/c11-class-similar-to-nets-manualresetevent-but-without-the-ability-to-rese
class NoResetEvent
{
public:
    NoResetEvent() : _state(false) {}
    NoResetEvent(const NoResetEvent& other) = delete;

    void WaitOne() {
        std::unique_lock<std::mutex> lock(_sync);
        while (!_state)
        {
            _underlying.wait(lock);
            _state = false;
            break;
        }
    }

    void Set() {
        std::unique_lock<std::mutex> lock(_sync);
        _state = true;
        _underlying.notify_one();
    }

private:
    std::condition_variable _underlying;
    std::mutex _sync;
    std::atomic<bool> _state;
};

