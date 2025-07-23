#include "Forest.h"
#include "Game.h"
#include "Weapon.h"
#include<iostream>
#include <fstream>

Forest::Forest(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox(), timer(START_TIME, TIME_POS, TIME_NAME)//reference to window
{
	setRoomName("Forest");
	setBackground("Forest1");
	//Need to load multiple images for multiples screens
	loadImages("Forest", 5);

	healthPoints = START_HEALTH_POINTS;
	healthBar = new HealthBar(START_HEALTH_POINTS, { 200.f, 15.f }, { 545.f, 70.f });

	//=== Dialogue System ===
	initializeVariables();
	//check if they came from kitchen or living room. They have to have teh grocery list if they came from the kitchen
	Item* i = new  Item("groceryList");
	if (inventory.hasItem(*i)) {
		currentIndex = 300;
		currentQuestionIndex = currentIndex;
	}
	delete i;

	// Initialise progress bar after initialising variables
	setRoomQuestions(11);
	progressBar = new ProgressBar(this->getRoomQuestions());

	//sound
	loadSounds();
}

Forest::~Forest() {
	delete progressBar;
	delete healthBar;

	// Destructor code
}

bool Forest::isHealthDepleted() {
	if (healthPoints <= 0) {
		return true;
	}
	return false;
}

void Forest::reduceHealthPoints(int change) {
	healthPoints -= change;
}

int Forest::getHealthPoints() {
	return healthPoints;
}

void Forest::check() {
	checkImages();

	if (currentIndex == 23) {
		displayHealthBar = true;
	}

	if (currentIndex == 3) {
		//check if inventory contains torch:

		Item* i = new  Item("Torch");

		if (inventory.hasItem(*i)) {
			currentIndex = 3;
		}
		else {
			currentIndex = 100;
		}

		delete i;



	}
	if (currentIndex == 4) {
		//check if inventory contains map:

		Item* i = new  Item("Map");

		//if has map:
		if (inventory.hasItem(*i)) {
			currentIndex = 4;
		}
		else {

			currentIndex = 100;

		}

		delete i;
	}

	if (currentIndex == 11) {
		//check if inventory contains torch:
		//if true:

		Item* i = new Item("Torch");

		if (inventory.hasItem(*i)) {
			currentIndex = 11;


		}
		else {

			currentIndex = 100;
		}

		delete i;
	}

	if (currentIndex == 10) {
		//check if inventory contains map:
		Item* i = new Item("Map");

		if (inventory.hasItem(*i)) {
			currentIndex = 10;
		}
		else {
			currentIndex = 100;
		}

		delete i;
	}

	if (currentIndex == 26) {
		//add axe to inventory
		Weapon i("axe");
		inventory.addItem(i);
	}

	if (currentIndex == 28) {
		reduceHealthPoints(2);
		healthBar->update(healthPoints); // Update the health bar
	}

	if (currentIndex == 102) {
		currentIndex = 28;
		reduceHealthPoints(1);
		healthBar->update(healthPoints); // Update the health bar
	}

	if (currentIndex == 29) {
		reduceHealthPoints(1);
		healthBar->update(healthPoints); // Update the health bar

	}
	if (currentIndex == 104) {
		currentIndex = 29;
		reduceHealthPoints(2);
		healthBar->update(healthPoints); // Update the health bar

	}

	if (currentIndex == 103) {
		//if you have the knife
		Weapon* i = new Weapon("knife");

		if (inventory.hasItem(*i)) {

			reduceHealthPoints(4);
			healthBar->update(healthPoints); // Update the health bar
			currentIndex = 29;
			inventory.useItem(*i);
		}
		else {

			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);

		}

		delete i;
	}

	if (currentIndex == 105) {
		//if you have the knife
		Weapon* i = new Weapon("knife");

		if (inventory.hasItem(*i)) {
			currentIndex = 30;
			reduceHealthPoints(2);
			healthBar->update(healthPoints); // Update the health bar
			inventory.useItem(*i);
		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;
	}

	if (currentIndex == 32) {
		//if you have the axe

		Weapon* i = new Weapon("axe");
		if (inventory.hasItem(*i)) {
			reduceHealthPoints(3);
			healthBar->update(healthPoints); // Update the health bar
		}
		else {

			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);

		}

		delete i;
	}

	if (currentIndex == 106) {
		//if you have the knife
		Weapon* i = new Weapon("knife");
		if (inventory.hasItem(*i)) {

			currentIndex = 32;
			reduceHealthPoints(2);
			healthBar->update(healthPoints); // Update the health bar
			inventory.useItem(*i);

		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;
	}

	if (currentIndex == 107) {
		currentIndex = 32;
		reduceHealthPoints(1);
		healthBar->update(healthPoints); // Update the health bar

	}

	if (currentIndex == 108) {
		//add bat to inventory
		Weapon i("bat");
		inventory.addItem(i);
		currentIndex = 33;

	}
	if (currentIndex == 34) {
		//if you have the bat

		Weapon* i = new Weapon("bat");

		if (inventory.hasItem(*i)) {
			reduceHealthPoints(4);
			healthBar->update(healthPoints); // Update the health bar
		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;
	}

	if (currentIndex == 109) {
		//if you have the axe
		Weapon* i = new Weapon("axe");

		if (inventory.hasItem(*i)) {
			currentIndex = 34;
			reduceHealthPoints(4);
			healthBar->update(healthPoints); // Update the health bar
		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;
	}

	if (currentIndex == 110) {
		//if you have the knife

		Weapon* i = new Weapon("knife");

		if (inventory.hasItem(*i)) {
			inventory.useItem(*i);
			currentIndex = 34;
			reduceHealthPoints(3);
			healthBar->update(healthPoints); // Update the health bar

		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;


	}
	if (currentIndex == 111) {

		currentIndex = 34;
		reduceHealthPoints(1);
		healthBar->update(healthPoints); // Update the health bar
	}

	if (currentIndex == 35) {
		//if you have the bat

		Weapon* i = new Weapon("bat");

		if (inventory.hasItem(*i)) {
			reduceHealthPoints(3);
			healthBar->update(healthPoints); // Update the health bar
		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}
		delete i;

	}
	if (currentIndex == 112) {
		//if you have the axe

		Weapon* i = new Weapon("axe");

		if (inventory.hasItem(*i)) {
			currentIndex = 35;
			reduceHealthPoints(3);
			healthBar->update(healthPoints); // Update the health bar
		}
		else {
			currentIndex = 100;
			progressBar->decrease();
			progressBar->decreaseAnsweredQuestions(1);
		}

		delete i;

	}
	if (currentIndex == 113) {

		currentIndex = 35;
		reduceHealthPoints(1);
		healthBar->update(healthPoints); // Update the health bar
	}

	if (currentIndex == 35) {
		if (isHealthDepleted()) {
			currentIndex = 36;
		}
		else {
			currentIndex = 37;
		}
	}

	if (currentIndex == 101) {
		currentIndex = prevPrevIndex;
	}

	playSounds();//play sounds

	//game over and lose if 0
	if (currentIndex == 0) {
		eraseSounds();
		game.setState(Game::State::GameOver);
		return;
	}

	//sets it to win if -1 in 3rd level/car
	if (currentIndex == -1) {
		eraseSounds();
		game.setWin(true);
		game.setState(Game::State::GameOver);
		return;
	}

	if (timer.getTimeLeft() <= 0) {
		currentIndex = 500;
	}

	if ((currentIndex == 36) || (currentIndex == 37) || (currentIndex == 31) || (currentIndex == 500)) {
		timer.stop();
	}

}



void Forest::handleInput(sf::Vector2i& mousePos) {
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
				//resets timer-based questions
				switch (currentQuestionIndex) {
				case 27:
				case 28:
				case 29:
				case 30:
				case 32:
				case 33:
				case 34:
					timer.reset(START_TIME);
					timer.start();
					break;
				default:
					break;
				}

				questionSet = false; //for the new question and optons to be set next time
				clickHandled = true; // BLOCK further clicks
			}
		}
	}
}

void Forest::update() {
	// Update logic for the Forest room
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
void Forest::render() {
	window.draw(backgroundImage);

	//=== DIALOGUE SYSTEM
	dialogueBox.render(window);
	if (displayHealthBar) {
		healthBar->draw(window); // Draw the health bar
	}
	// Render all items
	for (const auto& item : items) {
		window.draw(item);
	}

	// Render the inventory on the left side
	inventory.render(window);

	//render timers
	if (timer.isRunning()) {
		timer.render(window);
	}

	// Render the progress bar
	progressBar->draw(window);  // Draw the progress bar centred at the top
}

void Forest::playSounds()
{
	bool soundPlayed = false;
	//playing sounds at points in the forest when needed, this procedure is called in the check() procedure
	if ((currentIndex == 32) || (currentIndex==34 )) {
	Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "AttackSound3");
	soundPlayed = true;
}
if (currentIndex == 31) {
	Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "AttackSound2");
	soundPlayed = true;
}
if (currentIndex == 29) {
	Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "AttackSound1");
	soundPlayed = true;
}
if (currentIndex == 13) {
	Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "BearGrowl");
	soundPlayed = true;
}

if ((currentIndex == 9) || (currentIndex == 6)) {
	Audio::getInstance().playSound(SoundEffects::StageThreeSounds, "BearTrap");
	soundPlayed = true;
}

//if the continue button to next dialogue is clicked... the current sound playing will instantly stop
if (!soundPlayed)
{
	Audio::getInstance().stopSound("BearGrowl");
	Audio::getInstance().stopSound("BearTrap");
	Audio::getInstance().stopSound("AttackSound1");
	Audio::getInstance().stopSound("AttackSound2");
	Audio::getInstance().stopSound("AttackSound3");
}
}

void Forest::loadSounds()
{
	//removes stage two's ambience from memory so we can use forest ambience
	Audio::getInstance().eraseSound("StageTwoAmbienceSound");
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "ForestAmbience");
	Audio::getInstance().setSoundVolume("ForestAmbience", 20.0f);
	Audio::getInstance().loadSound(SoundEffects::StageThreeSounds);
	Audio::getInstance().loadSound(SoundEffects::Killer);


}

void Forest::eraseSounds()
{
	//erase all stage three sounds
	Audio::getInstance().eraseSound("BearGrowl");
	Audio::getInstance().eraseSound("BearTrap");
	Audio::getInstance().eraseSound("CarNotStarting");
	Audio::getInstance().eraseSound("CarStart");
	Audio::getInstance().eraseSound("CrowbarShedDoor");
	Audio::getInstance().eraseSound("ForestAmbience");
	//erase all killer sounds
	Audio::getInstance().eraseSound("AttackSound1");
	Audio::getInstance().eraseSound("AttackSound2");
	Audio::getInstance().eraseSound("AttackSound3");
}
