#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include <map>
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>
#include <iostream>
#include <fstream>

class Basement : public Room {
private:
    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    ProgressBar* progressBar;  // Progress bar for this room
    Inventory<Item>& inventory; // Reference to the inventory
    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically

    int burnCount; // Holds the number of burns the player gets from answering the wire riddle incorrectly
    bool isBurnt; // Checks whether the player is burnt and affects where they are directed in the gameplay
    const string correctKey = "KeyOne"; //The correct choice of keys

    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;

public:
    // Constructor and destructor
    Basement(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Basement();

    //Overrides of the parent class functions
    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    void intializeQuestionVariables();

    //Basement gameplay functions
    void burn(); // Burns the player - adjusts necessary variables
    bool getIsBurnt(); // Gets isBurnt
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly

    //basement audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
