#include "Stairs.h"
#include "Game.h"

#include<iostream>
#include <fstream>

Stairs::Stairs(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory) : window(window), game(game), inventory(inventory), currentQuestionIndex(1), dialogueBox()
{
    //need to get stairs image
    setRoomName("Stairs");
    setBackground("Stairs");

    //=== Dialogue System ===
    initializeVariables();

    //play ambience for the whole of stage 2
    playSounds();
}

Stairs::~Stairs()
{
}


void Stairs::handleInput(sf::Vector2i& mousePos)
{
    //==== DIALOGUE SYSTEM
    if (!clickHandled){
        int selected = dialogueBox.handleInput(mousePos); //returns index if choice clicked, -1 if not
        if (selected != -1 && questions.count(currentQuestionIndex)) { //valid option, and question at index
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

void Stairs::update()
{
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

void Stairs::render()
{
    window.draw(backgroundImage);
    //=== DIALOGUE SYSTEM
    dialogueBox.render(window);




    // Render all items
    for (const auto& item : items) {
        window.draw(item);
    }
    // Render the inventory on the left side
    inventory.render(window);
}

void Stairs::playSounds() {
    Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "StageTwoAmbienceSound");
    Audio::getInstance().setSoundVolume("StageTwoAmbienceSound", 70.0f);
}

void Stairs::check()
{
    if (currentIndex == -1) {
        ProgressBar::updateAnsweredQuestions(1); // Updates the answeredQuestions to account for the path chosen
        game.setState(Game::State::Bathroom);
        return;
    }

    if (currentIndex == -2) {
        ProgressBar::updateAnsweredQuestions(17); // Updates the answeredQuestions to account for the path chosen
        game.setState(Game::State::Kitchen);
        return;
    }

    if (currentIndex == -3) {
        ProgressBar::updateAnsweredQuestions(8); // Updates the answeredQuestions to account for the path chosen
        game.setState(Game::State::LivingRoom);
        return;
    }
}

void Stairs::intializeQuestionVariables()
{
    questionSet = false;
    clickHandled = true;
    currentQuestionIndex = 1;
    currentIndex = 1;
}
