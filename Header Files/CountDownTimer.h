#pragma once
#include <thread>
#include <atomic>
#include <mutex>
#include <SFML/Graphics.hpp>
#include "Audio.h"

using namespace std;
class CountDownTimer
{

private:
    int startTime;

    //atomic ensures safe access to variables in multithread environments
    atomic<int> timeLeft;
    atomic<bool> running;
    thread timerThread; //This is a background thread that runs the timer
    //garphics variables
    sf::Font textFont;
    sf::Text display;
    sf::Vector2f pos;
    string name;

    void run(); //This function will run in the background
    mutable mutex timeMutex; //Allows only one thread to access timeLeft method at a time
    string setText(); //sets text that will be displayed

    //audio methods
    void loadSounds();
public:
    CountDownTimer(int seconds, sf::Vector2f pos, string name); //seconds is the initial number of seconds
    ~CountDownTimer();
    CountDownTimer();

    void start();
    void stop();
    void reset(const int START_TIME);

    bool isRunning() const;
    int getTimeLeft() const;
    void reduceTime(int change);
    void render(sf::RenderWindow& window);

    CountDownTimer(const CountDownTimer&) = delete;  // Delete copy constructor
    CountDownTimer& operator=(const CountDownTimer&) = delete; // Delete copy assignment operator


};
