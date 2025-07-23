#include "LivingRoom.h"
#include "Game.h"

#include<iostream>
#include <fstream>

LivingRoom::LivingRoom(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox(), timer(START_TIME, TIME_POS, TIME_NAME), hideTimer(HIDE_TIME, TIME_POS, TIME_NAME)
{
    setRoomName("LivingRoom");
    setBackground("LivingRoom1");
    //multiple images
    loadImages("LivingRoom", 3);
    wrongCount = 0;
    floorboardCount = 0;
    drawerCount = 0;

    //=== Dialogue System ===
    initializeVariables();

    // Initialise progress bar after initialising variables
    setRoomQuestions(6);
    progressBar = new ProgressBar(this->getRoomQuestions());

    //sound
    loadSounds();
}

LivingRoom::~LivingRoom() {
    delete progressBar;
    // Destructor code
}

void LivingRoom::tooManyWrong() {
	if (wrongCount == 3) {
		currentIndex = 50;
	}
	return;
}

void LivingRoom::check() {
    playSounds();
    checkImages();

	if (currentIndex == 101) {
		currentIndex = prevPrevIndex;
	}
	if (currentIndex == 40) {
		//add tv remote to inventory
		Item i("Remote");
		inventory.addItem(i);
		currentIndex = 5;
	}
	if (currentIndex == 7 || currentIndex == 9 || currentIndex == 11 || currentIndex == 13 || currentIndex == 15 || currentIndex == 17) {
		wrongCount++;
		tooManyWrong();
	}

    if (currentIndex == 18) {
        //start timer for hiding
        hideTimer.start();
    }

    if ((currentIndex == 19) || (currentIndex == 20) || (currentIndex == 21)) {
        //stop hiding timer
        hideTimer.stop();
    }

	if (currentIndex == 22) {
		//timer starts
        timer.start();
	}

	if (currentIndex == 51) {
		//add torch to inventory
		Item i("Torch");
		inventory.addItem(i);
		currentIndex = 32;
		return;
	}
	if (currentIndex == 52) {
		//add carKeys to inventory
		Item i("CarKey");
		inventory.addItem(i);
		currentIndex = 33;
		return;
	}
	if (currentIndex == 29) {
		if (drawerCount == 0) {
			drawerCount++;
		}
		else {
			currentIndex = 100;
		}
	}
	if (currentIndex == 31) {
		if (floorboardCount == 0) {
			floorboardCount++;
		}
		else {
			currentIndex = 100;
		}
	}
	if (currentIndex == 37) {
		//stop timer
		//GRAPHICS
        timer.stop();
	}
    
    //move to car or forest based on the items in the inventory
    if (currentIndex == -1) {
        eraseSounds();
        Item i("CarKey");
        if (inventory.hasItem(i)) {
            progressBar->updateAnsweredQuestions(11); // Updates the answeredQuestions to account for the path chosen
            game.setState(Game::State::Car);
        }
        else {
            progressBar->updateAnsweredQuestions(9); // Updates the answeredQuestions to account for the path chosen
            game.setState(Game::State::Forest);
        }
        return;
    }

    if (currentIndex == 0) {
        eraseSounds();
        game.setState(Game::State::GameOver);
    }
    //if you run out of time while hiding
    if (hideTimer.getTimeLeft() <= 0) {
        currentIndex = 200;
    }

    //if timer runs out then currentIndex = 27
    //GRAPHICS
    if (timer.getTimeLeft() <= 0) {
        currentIndex = 27;

    }
}

void LivingRoom::handleInput(sf::Vector2i& mousePos) {
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

void LivingRoom::update() {
    // Update logic for the living room
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
void LivingRoom::render() {
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

    //render timer
    if (timer.isRunning()){
        timer.render(window);
    }

    if (hideTimer.isRunning()) {
        hideTimer.render(window);
    }
    
}

void LivingRoom::playSounds()
{
    bool soundPlayed = false;
    //playing sounds at points in the living room when needed, this procedure is called in the check() procedure
    if ((currentIndex == 19) || (currentIndex == 20) || (currentIndex == 21)) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "FootstepsThenOpeningDoor");
        soundPlayed = true;
    }

    if ((currentIndex == 3) || (currentIndex == 29)) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "OpeningDesk");
        soundPlayed = true;
    }

    if (currentIndex == 35) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "OpeningSafe");
        soundPlayed = true;
    } 

    if (currentIndex == 18) { 
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "TVStatic");
        soundPlayed = true;
    }

    if ((currentIndex == 7) || (currentIndex == 9) || (currentIndex == 11) || (currentIndex == 13) || (currentIndex == 15) || (currentIndex == 17)) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "WireIncorrect");
        soundPlayed = true;
    }

    if (currentIndex == 19) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "CurtainOpening");
        soundPlayed = true;
    }

    //if the continue button to next dialogue is clicked... the current sound playing will instantly stop
    if (!soundPlayed)
    {
        Audio::getInstance().stopSound("FootstepsThenOpeningDoor");
        Audio::getInstance().stopSound("OpeningDesk");
        Audio::getInstance().stopSound("OpeningSafe");
        Audio::getInstance().stopSound("TVStatic");
        Audio::getInstance().stopSound("WireIncorrect");
        Audio::getInstance().stopSound("CurtainOpening");
    }
}

void LivingRoom::loadSounds()
{
    Audio::getInstance().loadSound(SoundEffects::StageTwoSounds);
}

void LivingRoom::eraseSounds()
{
    //erase all stage two sounds
    Audio::getInstance().eraseSound("IdleKettle");
    Audio::getInstance().eraseSound("IdleStove");
    Audio::getInstance().eraseSound("KettleScreech");
    Audio::getInstance().eraseSound("KillerCollapse");
    Audio::getInstance().eraseSound("GlassBreaking");
    Audio::getInstance().eraseSound("CurtainOpening");
    Audio::getInstance().eraseSound("FootstepsThenOpeningDoor");
    Audio::getInstance().eraseSound("OpeningDesk");
    Audio::getInstance().eraseSound("OpeningSafe");
    Audio::getInstance().eraseSound("TVStatic");
    Audio::getInstance().eraseSound("WireIncorrect");
    Audio::getInstance().eraseSound("RattlingLockedDoor");
}
