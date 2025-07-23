#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include "CountDownTimer.h"
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>

class LivingRoom : public Room {
private:
    int wrongCount; // Counts number of incorrect answers the player has selected
    int floorboardCount; // ==========NEEDS COMMENTS===============
    int drawerCount; 
    //timer variables
    int const START_TIME = 60;
    sf::Vector2f TIME_POS = { 1100.f, 20.f };
    string TIME_NAME = "Timer";
    int const HIDE_TIME = 20;

    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    Inventory<Item>& inventory; // Reference to the inventory
    ProgressBar* progressBar;  // Progress bar for this room
    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically
    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;
public:
    LivingRoom(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~LivingRoom();

    void tooManyWrong(); // Checks if player has selected too mnay incorrect answers and directs them accordingly
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly
    LivingRoom();

    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    //timer
    CountDownTimer timer;
    CountDownTimer hideTimer;

    //living room audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
