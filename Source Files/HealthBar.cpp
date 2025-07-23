#include "HealthBar.h"
#include <algorithm> // For std::max

// Constructor to initialize health bar attributes
HealthBar::HealthBar(int maxHealth, sf::Vector2f size, sf::Vector2f position)
    : maxHealth(maxHealth), currentHealth(maxHealth), maxWidth(size.x) {

    // Initialize the bar outline (background)
    float spacing = 10.f; // Space between progress and health bars
    barOutline.setSize(size);
    barOutline.setFillColor(sf::Color::Transparent);
    barOutline.setOutlineThickness(3.f);
    barOutline.setOutlineColor(sf::Color::White); // White outline
    barOutline.setPosition(position);

    // Initialize the filled health bar
    barFill.setSize(size);
    barFill.setFillColor(sf::Color::Red); // Red for health
    barFill.setPosition(position);

    // Load broken heart texture
    if (!heartTexture.loadFromFile("Assets/Icons/BrokenHeart.png")) {
        throw std::runtime_error("Failed to load broken heart icon.");
    }

    // Setup sprite for the broken heart icon
    heartSprite.setTexture(heartTexture);
    heartSprite.setScale(0.5f, 0.5f); // Scale down if needed
    heartSprite.setPosition({ position.x - 35.f, position.y }); // Positioned slightly to the left of the health bar
}

// Updates the health bar based on current health points
void HealthBar::update(int healthPoints) {
    currentHealth = std::max(0, healthPoints); // Ensure health doesn't go below 0
    float healthPercentage = static_cast<float>(currentHealth) / maxHealth; // Calculate health percentage
    barFill.setSize({ maxWidth * healthPercentage, barFill.getSize().y }); // Adjust the width of the filled bar
}

// Draws the health bar on the given window
void HealthBar::draw(sf::RenderWindow& window) {
    window.draw(heartSprite);      // Draw the broken heart icon
    window.draw(barOutline); // Draw the outline
    window.draw(barFill);    // Draw the filled portion
}
