#include "Basement.h"
#include "Game.h"

Basement::Basement(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory)
    : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox()
{   
    //setting room and background image
    setRoomName("Basement");
    setBackground("Basement1");
    loadImages("Basement", 2);
    //intilaize burn variables
    burnCount = 0;
    isBurnt = false;

	//=== Dialogue System ===
    initializeVariables();

    // Initialise progress bar after initialising variables
    setRoomQuestions(8);
    progressBar = new ProgressBar(this->getRoomQuestions());

    //Loading Audio files
    loadSounds();

}

Basement::~Basement() {
    // Destructor code
    delete progressBar;
}

void Basement::loadSounds()
{
    Audio::getInstance().loadSound(SoundEffects::StageOneSounds);
}

void Basement::burn() {
    burnCount++;
    isBurnt = true;
}

bool Basement::getIsBurnt() {
	return isBurnt;
}

void Basement::playSounds()
{
    bool soundPlayed = false;
    //playing sounds at points in the basement when needed, this procedure is called in the Basement::check() procedure below
    if (currentIndex == 2) {
        Audio::getInstance().playSound(SoundEffects::StageOneSounds, "electricsparks1");
        soundPlayed = true;
    }
    if (currentIndex == 4) {
        Audio::getInstance().playSound(SoundEffects::StageOneSounds, "FemaleHurt1");
        soundPlayed = true;
    }

    if (currentIndex == 37) {
        Audio::getInstance().playSound(SoundEffects::StageOneSounds, "FeintKeyRattleInBox");
        soundPlayed = true;
    }
    if (currentIndex == 36) {
        Audio::getInstance().playSound(SoundEffects::StageOneSounds, "LightTurnsOn");
        soundPlayed = true;
    }
    if (currentIndex == 53) {
        Audio::getInstance().playSound(SoundEffects::StageOneSounds, "CreakingDoor");
        soundPlayed = true;
    }

    //if the continue button to next dialogue is clicked... the current sound playing will instantly stop
    if (!soundPlayed)
    {
        Audio::getInstance().stopSound("electricsparks1");
        Audio::getInstance().stopSound("LightTurnsOn");
        Audio::getInstance().stopSound("FemaleHurt1");
        Audio::getInstance().stopSound("FeintKeyRattleInBox");
        Audio::getInstance().stopSound("CreakingDoor");

    }

}
void Basement::eraseSounds()
{
    //when changing game states..we can erase all these sounds
    Audio::getInstance().eraseSound("electricsparks1");
    Audio::getInstance().eraseSound("LightTurnsOn");
    Audio::getInstance().eraseSound("FemaleHurt1");
    Audio::getInstance().eraseSound("FeintKeyRattleInBox");
    Audio::getInstance().eraseSound("CreakingDoor");

    //erasing the backgroundsound since stage one is finished
    Audio::getInstance().eraseSound("StartingAmbience");

}
void Basement::check() {
    checkImages();
    playSounds(); //plays sound for question
	if (currentIndex == 4) {
		burnCount++;
		return;
	}
	if (currentIndex == 36 && burnCount != 0) {
		currentIndex = 5;
		return;
	}
	if (currentIndex == 35) {
		burnCount--;
	}
	if (currentIndex == 100) {
		if (burnCount != 0) {
			currentIndex = prevPrevIndex + 1;
		}
		else if (burnCount == 0) {
			currentIndex = 36;
		}
		return;
	}
	if (currentIndex == 52) {
		//add key to inventory
		Item i("goldKey");
		inventory.addItem(i);
		return;
	}
	if (currentIndex == 53) {
		//add key to inventory
		Item i("silverKey");
		inventory.addItem(i);
		return;
	}
    if (currentIndex == 54) {
        //remove key from inventory
        Item i2("goldKey");
        inventory.useItem(i2);
        return;
    }
	if (currentIndex == 64) {
		//add key to inventory
		Item i("silverKey");
		inventory.addItem(i);
		currentIndex = 62;
		return;
	}
	if (currentIndex == 63) {
		//add key to inventory
		Item i("goldKey");         
		inventory.addItem(i);
		return;
	}
	if (currentIndex == 61) {
		//remove key from inventory
		Item i("goldKey");
		inventory.useItem(i);
		return;
	}
	if (currentIndex == -1) {
        eraseSounds();
		//move to exit basement
        game.setState(Game::State::Stairs);
		return;
	}

    if (currentIndex == 0) {
        eraseSounds();
        game.setState(Game::State::GameOver);
    }

}

void Basement::handleInput(sf::Vector2i& mousePos) {
    // Handle input events here
    //==== DIALOGUE SYSTEM
    if (!clickHandled){
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

void Basement::update() {
    // Update logic for the basement room
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

//draw room and objects
void Basement::render() {
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
}

//allows to game to reset basement without having to construct a new room
void Basement::intializeQuestionVariables()
{
    questionSet = false;
    clickHandled = true;
    currentQuestionIndex = 1;
    currentIndex = 1;
}
