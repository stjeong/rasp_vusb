#pragma once

#include <queue>
using namespace std;

// thread-safe queue

template <class Type>
class QueueTS
{
public:
    QueueTS(int maxQueue)
    {
        _maxQueue = maxQueue;
    }

    size_t Size()
    {
        return _queue.size();
    }

    Type Get()
    {
        std::unique_lock<std::mutex> lock(_sync);

        if (_queue.size() == 0)
        {
            return Type();
        }

        Type instance = _queue.front();
        _queue.pop();

        return instance;
    }

    void Add(Type instance)
    {
        std::unique_lock<std::mutex> lock(_sync);

        if (_queue.size() > _maxQueue)
        {
            _queue.pop();
        }

        _queue.push(instance);
    }

private:
    queue<Type> _queue;
    std::mutex _sync;
    size_t _maxQueue;
};