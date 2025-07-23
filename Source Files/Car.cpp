#include "Car.h"
#include "Game.h"

#include<iostream>
#include <fstream>

Car::Car(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox(), timer(START_TIME, TIME_POS, TIME_NAME), fuelTimer(START_TIME_FUEL, TIME_FUEL_POS, TIME_FUEL_NAME)//reference to window
{
	setRoomName("Car");
	setBackground("Car1");
   //Load 2 car images for 2 different screens
	loadImages("Car",3);
	
    fuelCount = 0;
	//=== Dialogue System ===
	initializeVariables();

	// Initialise progress bar after initialising variables
	setRoomQuestions(9);
	progressBar = new ProgressBar(this->getRoomQuestions());

	//sound
	loadSounds();
}

Car::~Car() {
	delete progressBar;
    // Destructor code
}

void Car::fillFuel() {
    fuelCount++;
}

bool Car::isFull() {
    if (fuelCount >= FUELNEEDED) {
        return true;
    }
    return false;
}

void Car::check() {
	playSounds();
	checkImages();
	if (currentIndex == 2) {
		timer.start();
	}

	if (currentIndex == 101) {
		currentIndex = prevPrevIndex;
	}
	
	if (currentIndex == 68) { //ADDED Phone booting up
		timer.reduceTime(20);
		return;
	}

	if (currentIndex == 13) {
		

		Item* i = new Item("crowbar");

		if (!inventory.hasItem(*i)) {
			currentIndex = 100;
		}
		else {
			timer.reduceTime(10); //ADDED using crowbar
		}

		delete i;
		return;

	}
	if (currentIndex == 14) {
		timer.reduceTime(25); //ADDED 
		return;
	}


	if (currentIndex == 60) {
		//add crowbar to inventory
		Item i("crowbar");
		inventory.addItem(i);
		currentIndex = 10;
	}
	if (currentIndex == 61) {
		// add trunkKey to inventory
		Item i("goldKey");
		inventory.addItem(i);
		currentIndex = 12;
	}
	if (currentIndex == 15) {
		//if inventory does NOT contain trunkKey

		Item* i = new Item("goldKey");

		if (!inventory.hasItem(*i)) {
			currentIndex = 100;
		}

		delete i;
		return;


	}
	if (currentIndex == 13) {
		//if inventory contains crowbar
		// 
		Item* i = new Item("crowbar");
		if (inventory.hasItem(*i)) {

			inventory.useItem(*i);
			//remove crowbar from inventory
			timer.reduceTime(10);
		}
		else {
			currentIndex = 100;
		}

		delete i;
		return;
	}
	if (currentIndex == 17) {
		// FuelTimer = 2mins
		fuelTimer.start();
	}
	if (currentIndex == 35) {
		//fuelTimer can stop
		fuelTimer.stop();

	}
	//
	

	if (currentIndex == 37) {
		//if inventory does NOT contain trunkKey then:
		Item* i = new Item("goldKey");

		if (!inventory.hasItem(*i)) {
			currentIndex = 100;
		}
		else {
			//the inventory does contain trunkKey
			//timer delay of 10secs
			inventory.useItem(*i);
			timer.reduceTime(10);
		}

		delete i;
		return;

	}
	if (currentIndex == 40) {
		//timer delay of 25secs
		timer.reduceTime(25);
		return;
	}
	if (currentIndex == 41) {
		//if inventory contains torch
		Item* i = new Item("Torch");

		if (inventory.hasItem(*i)) {
			//timer delays of 5secs
			timer.reduceTime(5);
		}
		else {
			currentIndex = 100;
		}
		delete i;
		return;
	}

	if (currentIndex == 42) {

		//timer delay of 15secs
		timer.reduceTime(15);
	}
	if (currentIndex == 43) {
		//timer delay of 20secs
		timer.reduceTime(20);

	}
	if (currentIndex == 46) {
		//timer delay of 10secs	
		timer.reduceTime(10);
	}
	if (currentIndex == 44) {
		//timer delay of 10secs
		timer.reduceTime(10);
	}
	if (currentIndex == 49) {
		//timer delay of 20secs
		timer.reduceTime(20);
	}

	if ((currentIndex == 7) || (currentIndex == 36) || (currentIndex == 48) || (currentIndex == 50)) {
		if (timer.isRunning()) {
			timer.stop();
		}
	}


	//game over and lose if 0
	if (currentIndex == 0) {
		eraseSounds();
		game.setState(Game::State::GameOver);
	}

	//sets it to win if -1 in 3rd level/car
	if (currentIndex == -1) {
		eraseSounds();
		game.setWin(true);
		game.setState(Game::State::GameOver);
	}

	if (timer.getTimeLeft() <= 0) {
		currentIndex = 50;
	}

	if (fuelTimer.getTimeLeft() <= 0) {
		currentIndex = 36;

	}

}

void Car::handleInput(sf::Vector2i& mousePos) {
    // Handle input events here

	//==== DIALOGUE SYSTEM
	if (!clickHandled) {
		int selected = dialogueBox.handleInput(mousePos); //returns index if choice clicked, -1 if not
		if (selected != -1 && questions.count(currentQuestionIndex)) { //valid option, and question at index
			// Checks if we need to update the progress bar
			if (questions.count(currentQuestionIndex)) {
				Option selectedOption = questions[currentQuestionIndex].getOptions()[selected];
				if (selectedOption.getProgress()) {
					progressBar->update();
					progressBar->updateAnsweredQuestions(1);
				}
			}

			//the selected option decides the next question (chooseOption)
			int nextIndex = questions[currentQuestionIndex].chooseOption(selected);
			moveToNextQuestion(nextIndex);
			check();
			if (questions.count(currentIndex)) { //if next question exists
				currentQuestionIndex = currentIndex; //update index
				questionSet = false; //for the new question and optons to be set next time
				clickHandled = true; // BLOCK further clicks
			}
		}
	}
}

void Car::update() {
    // Update logic for the Car room
    // Get the current mouse position relative to the window
    sf::Vector2i mousePos = sf::Mouse::getPosition(window);

    // Check if the left mouse button is pressed
    if (sf::Mouse::isButtonPressed(sf::Mouse::Left)) {
        handleInput(mousePos);
    }

	//==== DIALOGUE SYSTEM
	if (!questionSet && questions.count(currentQuestionIndex)) { //if question is not set yet, and if theres a question for the given index
		//set text in dialoguebox to the question's prompt
		dialogueBox.setText(questions[currentQuestionIndex].getPrompt());
		//set the options for the current question
		dialogueBox.setChoices(questions[currentQuestionIndex].getOptions());
		questionSet = true; //make true so it doesn't run again for same question
	}

	// reset clickHandled when the mouse is no longer pressed
	if (!sf::Mouse::isButtonPressed(sf::Mouse::Left)) {
		clickHandled = false;
	}
}

//Create the window
void Car::render() {
    window.draw(backgroundImage);

	//=== DIALOGUE SYSTEM
	dialogueBox.render(window);
    // Render all items
    for (const auto& item : items) {
        window.draw(item);
    }
    // Render the inventory on the left side
    inventory.render(window);

	// Render the progress bar
	progressBar->draw(window);  // Draw the progress bar centred at the top

	//render timers
	if (timer.isRunning()) {
		timer.render(window);
	}
	
	if (fuelTimer.isRunning()) {
		fuelTimer.render(window);
	}
}

void Car::playSounds()
{
	bool soundPlayed = false;
	//playing sounds at points in the car when needed, this procedure is called in the check() procedure
	if ((currentIndex == 2) || (currentIndex == 47)) {
		Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "CarNotStarting");
		soundPlayed = true;
	}

	if (currentIndex == 48) {
		Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "CarStart");
		soundPlayed = true;
	}

	if (currentIndex == 13) {
		Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "CrowbarShedDoor");
		soundPlayed = true;
	}

	//if the continue button to next dialogue is clicked... the current sound playing will instantly stop
	if (!soundPlayed)
	{
		Audio::getInstance().stopSound("CarNotStarting");
		Audio::getInstance().stopSound("CarStart");
		Audio::getInstance().stopSound("CrowbarShedDoor");
	}
}

void Car::loadSounds()
{
	Audio::getInstance().eraseSound("StageTwoAmbienceSound");
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "CarAmbience");
	Audio::getInstance().loadSound(SoundEffects::StageThreeSounds);
}

void Car::eraseSounds()
{
	//erase all stage three sounds
	Audio::getInstance().eraseSound("BearGrowl");
	Audio::getInstance().eraseSound("BearTrap");
	Audio::getInstance().eraseSound("CarNotStarting");
	Audio::getInstance().eraseSound("CarStart");
	Audio::getInstance().eraseSound("CrowbarShedDoor");
	Audio::getInstance().eraseSound("CarAmbience");
}
