#include "Bathroom.h"
#include "Game.h"
#include<iostream>
#include <fstream>

Bathroom::Bathroom(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox()//reference to window
{
    setRoomName("Bathroom");
    setBackground("Bathroom");

    //=== Dialogue System ===
    initializeVariables();

    // Initialise progress bar after initialising variables
    setRoomQuestions(2);
    progressBar = new ProgressBar(this->getRoomQuestions());

    //Loading Audio files
    loadSounds();
}

Bathroom::~Bathroom() {
    delete progressBar;
    // Destructor code
}


void Bathroom::handleInput(sf::Vector2i& mousePos) {
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

void Bathroom::update() {
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

//Create the window
void Bathroom::render() {
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

//checks question we at to change rooms, add items to index etc.
void Bathroom::check()
{
    playSounds();
    if (currentIndex == 3) {
        eraseSounds();
        progressBar->updateAnsweredQuestions(-1); // Accounts for backtracking
        game.setState(Game::State::Stairs);
        return;
    }

    if (currentIndex == 0) {
        eraseSounds();
        game.setState(Game::State::GameOver);
    }
}

void Bathroom::intializeQuestionVariables()
{
    questionSet = false;
    clickHandled = true;
    currentQuestionIndex = 1;
    currentIndex = 1;
    loadSounds(); //ensures sounds work if you re-enter room
}

void Bathroom::playSounds()
{
    bool soundPlayed = false;
    //playing sounds at points in the bathroom when needed, this procedure is called in the check() procedure
    if ((currentIndex == 6) || (currentIndex == 7)) {
        Audio::getInstance().playSound(SoundEffects::StageTwoSounds, "GlassBreaking");
        soundPlayed = true;
    }

    //if the continue button to next dialogue is clicked... the current sound playing will instantly stop
    if (!soundPlayed)
    {
        Audio::getInstance().stopSound("GlassBreaking");
    }
}

void Bathroom::loadSounds()
{
    Audio::getInstance().loadSound(SoundEffects::StageTwoSounds);
}

void Bathroom::eraseSounds()
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
}
