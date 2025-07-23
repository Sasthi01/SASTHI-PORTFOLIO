#pragma once
#include "Room.h"
#include "Inventory.h"
#include "DialogueBox.h"
#include "ProgressBar.h"
#include "CountDownTimer.h"
#include <vector> // Include vector for dynamic item storage
#include <SFML/Graphics.hpp>

class Car : public Room {
private:
    sf::RenderWindow& window;  // Reference to the window
    Game& game;                // Reference to the game
    Inventory<Item>& inventory; // Reference to the inventory
    ProgressBar* progressBar;  // Progress bar for this room

    //timer variables
    const int START_TIME = 300;
    sf::Vector2f TIME_POS = { 1100.f, 20.f };
    string TIME_NAME = "Timer";
    const int START_TIME_FUEL = 120;
    sf::Vector2f TIME_FUEL_POS = { 1075.f, 50.f };
    string TIME_FUEL_NAME = "Fuel timer";

    static const int maxItems = 5; // Maximum number of items
    std::vector<sf::RectangleShape> items; // Use a vector to store items dynamically

    int fuelCount; // The number of fuel fills completed
    const int FUELNEEDED = 5; // The number of fuel fills needed

    // === Dialogue system ===
    DialogueBox dialogueBox;
    int currentQuestionIndex;
    bool questionSet = false;
    bool clickHandled = true;
public:
    Car(sf::RenderWindow& window, Game& game, Inventory<Item>& inventory);
    ~Car();

    void fillFuel(); // Increases fuel fills completed
    bool isFull(); //Checks if fuel is full
    Car();
    void check() override; // Gameplay function - checks question index in text files and directs player accordingly
    // Overrides for event handling and graphics displaying
    void handleInput(sf::Vector2i& mousePos) override;
    void update() override;
    void render() override;

    //timer variables
    CountDownTimer timer;
    CountDownTimer fuelTimer;

    //car audio procedures
    void playSounds();
    void loadSounds();
    void eraseSounds();
};
