#include "CountDownTimer.h"
#include <iostream>
#include <chrono>
#include <thread>


using namespace std;

void CountDownTimer::loadSounds()
{
	//loading sound for timer
	Audio::getInstance().loadSound(BackgroundAudio::BackgroundSounds);
}

CountDownTimer::CountDownTimer(int seconds, sf::Vector2f pos, string name) {
	startTime = seconds;
	timeLeft = seconds;
	this->pos = pos;
	this->name = name;
	running = false;
	textFont.loadFromFile("Assets/Fonts/roboto/Roboto-Regular.ttf");
	loadSounds();
}

CountDownTimer::CountDownTimer() {
	startTime = 0;
	timeLeft = 0;
	running = false;
	textFont.loadFromFile("Assets/Fonts/roboto/Roboto-Regular.ttf");
}

CountDownTimer::~CountDownTimer() {
	stop();
	if (timerThread.joinable()) {
		timerThread.join();
	}
}

void CountDownTimer::start() {
	if (running) {
		return;
	}

	running = true;
	timerThread = std::thread(&CountDownTimer::run, this); //Start the background task
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "ClockTicking");
	Audio::getInstance().setSoundVolume("ClockTicking", 20.0f);
}

void CountDownTimer::stop() {
	running = false;
	Audio::getInstance().stopSound("ClockTicking");
}

void CountDownTimer::reset(const int START_TIME) {
	startTime = START_TIME;
	timeLeft = START_TIME;
}

//Check if timer is running
bool CountDownTimer::isRunning() const {
	return running;
}

//Get the remaining time
int CountDownTimer::getTimeLeft() const {
	lock_guard<mutex> lock(timeMutex); 
	//Mutex will lock the timeMutex variable for mutual exclusion
	return timeLeft;
}

void CountDownTimer::run() {



	while (timeLeft > 0 && running) {
		{

			lock_guard<mutex> lock(timeMutex); //Will release lock when it goes out of scope
			--timeLeft;
		}
		this_thread::sleep_for(chrono::seconds(1));//Will sleep for one second
	}

	{

		lock_guard<mutex> lock(timeMutex); //Will release lock when it goes out of scope

		if (running && timeLeft == 0) {
			Audio::getInstance().stopSound("ClockTicking");
		}

		running = false;
	}
}

string CountDownTimer::setText()
{
	return name + " " + to_string(getTimeLeft() /60) + ":" + to_string(getTimeLeft() % 60);
}
//Will reduce the timer amount by the value indicated
void CountDownTimer::reduceTime(int change) {

	lock_guard<mutex> lock(timeMutex); //Will release lock when it goes out of scope

	if (change <= timeLeft) {
		timeLeft -= change;
	}
	else {
		timeLeft = 0;
	}

}

void CountDownTimer::render(sf::RenderWindow& window)
{
	display.setFont(textFont);
	display.setString(setText());
	display.setCharacterSize(30);
	display.setFillColor(sf::Color::White);
	display.setPosition(pos);

	window.draw(display);
}
