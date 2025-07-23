#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include <map>
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>

class Stairs : public Room {
private:
    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    Inventory<Item>& inventory; // Reference to the inventory
    //NEW
    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically
    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;
public:
    // Constructor and destructor
    Stairs(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Stairs();

    //Overrides of the parent class functions
    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    void check() override; // Gameplay function - checks question index in text files and directs player accordingly

    //method to ensure proper reentry of room
    void intializeQuestionVariables();

    //ambience for stage 2
    void playSounds();
};
