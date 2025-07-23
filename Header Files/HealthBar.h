#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Window.hpp>

// Class to represent a health bar for visualizing health points
class HealthBar {
private:
    int maxHealth;           // Maximum health points
    int currentHealth;       // Current health points
    float maxWidth;          // Full width of the health bar

    sf::RectangleShape barOutline;  // Outline of the health bar
    sf::RectangleShape barFill;     // Filled portion representing current health

    sf::Texture heartTexture;       // Texture for the broken heart icon
    sf::Sprite heartSprite;         // Sprite to render the broken heart icon

public:
    HealthBar(int maxHealth, sf::Vector2f size, sf::Vector2f position); // Constructor to initialize the health bar

    // Updates the health bar based on the current health points
    void update(int healthPoints);

    // Renders the health bar to the given window
    void draw(sf::RenderWindow& window);


};

