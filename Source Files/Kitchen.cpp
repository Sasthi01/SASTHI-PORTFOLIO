#include "Kitchen.h"
#include "Game.h"

#include <iostream>
#include <fstream>

Kitchen::Kitchen(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox()//reference to window
{
    setRoomName("Kitchen");
    setBackground("Kitchen1");
    loadImages("Kitchen", 2);
    wrongCount = 0;

    //=== Dialogue System ===
    initializeVariables();


    // Initialise progress bar after initialising variables
    setRoomQuestions(6);
    progressBar = new ProgressBar(this->getRoomQuestions());

    //loads audio
    loadSounds();
}

Kitchen::~Kitchen() {
    delete progressBar;
    // Destructor code
}

void Kitchen::tooManyWrong() {
    if (wrongCount == 3) {
        currentIndex = 28;
    }
    return;
}

void Kitchen::check() {
    playSounds();
    checkImages();
    if (currentIndex == 101) {
        currentIndex = prevPrevIndex;
    }
    if (currentIndex == 30) {
        //add map to inventory

        Item i("Map");
        inventory.addItem(i);
        currentIndex = 7;
        return;
    }
    if (currentIndex == 31) {
        // add grocery list to inventory
        Item i("groceryList");
        inventory.addItem(i);
        currentIndex = 7;
        return;
    }
    if (currentIndex == 32) {
        //add map to inventory
        //add grocery list to inventory
        Item i("Map");
        Item i2("groceryList");
        inventory.addItem(i);
        inventory.addItem(i2);
        currentIndex = 7;
        return;
    }
    if (currentIndex == 8) {
        //if inventory does NOT contain grocery list

        Item* i = new Item("groceryList");

        if (!inventory.hasItem(*i)) {
            setBackground("Kitchen1");
            currentIndex = 100;
        }
        delete i;
        return;
    }
    if (currentIndex == 9 || currentIndex == 13 || currentIndex == 16 || currentIndex == 19) {
        wrongCount++;
        tooManyWrong();
        return;
    }
    if (currentIndex == 35) {
        //add knife to inventory

        Item i("knife");
        inventory.addItem(i);
        currentIndex = 26;
        return;
    }
    //move to forest
    if (currentIndex == -1) {
        eraseSounds();
        game.setState(Game::State::Forest);
        return;
    }

    if (currentIndex == 0) {
        eraseSounds();
        game.setState(Game::State::GameOver);
    }
}


void Kitchen::handleInput(sf::Vector2i& mousePos) {
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

void Kitchen::update() {
    // Update logic for the Kitchen room
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
void Kitchen::render() {
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

void Kitchen::playSounds()
{
    bool soundPlayed = false;
    //playing sounds at points in the kitchen when needed, this procedure is called in the check() procedure
    if (currentIndex == 6) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "IdleKettle");
        soundPlayed = true;
    }

    if (currentIndex == 2) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "IdleStove");
        soundPlayed = true;
    }

    if ((currentIndex == 24) || (currentIndex == 25) || (currentIndex == 27) || (currentIndex == 28)) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "KettleScreech");
        soundPlayed = true;
    }

    if (currentIndex == 24) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "KillerCollapse");
        soundPlayed = true;
    }

    if (currentIndex == 3) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "RattlingLockedDoor");
        soundPlayed = true;
    }

    //if the continue button to next dialogue is clicked... the current sound playing will instantly stop
    if (!soundPlayed)
    {
        Audio::getInstance().stopSound("IdleKettle");
        Audio::getInstance().stopSound("IdleStove");
        Audio::getInstance().stopSound("KettleScreech");
        Audio::getInstance().stopSound("KillerCollapse");
        Audio::getInstance().stopSound("RattlingLockedDoor");
    }
}

void Kitchen::loadSounds()
{
    Audio::getInstance().loadSound(SoundEffects::StageTwoSounds);
}

void Kitchen::eraseSounds()
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
