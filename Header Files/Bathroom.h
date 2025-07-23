#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include "Stairs.h"
#include <map>
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>

class Bathroom : public Room {
private:
    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    ProgressBar* progressBar;  // Progress bar for this room
    Inventory<Item>& inventory; // Reference to the inventory
    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically
    

    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;

public:
    Bathroom(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Bathroom();


    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly

    //method to ensure proper reentry of room
    void intializeQuestionVariables();

    //bathroom audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
