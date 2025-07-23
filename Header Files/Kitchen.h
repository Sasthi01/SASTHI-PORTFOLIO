#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>

class Kitchen : public Room {
private:
    int wrongCount; //Counts the number of incorrect answers the player selects

    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    Inventory<Item>& inventory; // Reference to the inventory
    ProgressBar* progressBar;  // Progress bar for this room
    //NEW
    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically

    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;
public:
    Kitchen(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Kitchen();

    Kitchen();
    void tooManyWrong(); //Checks if the player has selected too many wromg answers
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly

    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    //kitchen audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
