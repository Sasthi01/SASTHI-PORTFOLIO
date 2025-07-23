#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include "CountDownTimer.h"
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>
#include "HealthBar.h"

class Forest : public Room {
private:
    // Start and current running health points of the kidnapper
    int healthPoints;
    const int START_HEALTH_POINTS = 10;
    HealthBar* healthBar;
    bool displayHealthBar = false;

    //timer variables
    const int START_TIME = 10;
    sf::Vector2f TIME_POS = { 1100.f, 20.f };
    string TIME_NAME = "Timer";

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
    Forest(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Forest();

    int getHealthPoints(); // Gets the health points of the kidnapper
    void reduceHealthPoints(int change); // Reduces the health points based on the player's actions
    bool isHealthDepleted(); // Checks if the health of the kidnhapper has been depleted enough to defeat him
    Forest();
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly

    //timer variables
    CountDownTimer timer;

    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    //forest audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
