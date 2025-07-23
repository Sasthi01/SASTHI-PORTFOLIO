#pragma once
#include "Item.h"
#include <vector>
#include <SFML/Graphics.hpp>
#include <iostream>
#include <string>
#include <algorithm>
template <typename T>
class Inventory
{

private:
	// Vector to track items
	vector<T> items;
	int numItems = 0;
	const int CAPACITY = 5;
	sf::Text fullText;
	sf::Font fullFont;

public:
	// Add an item to the inventory for a specific room
	bool addItem(const T& i); //Returns false if the items cannot be added to the list ie the inventory is full
	// Check if an item exists in the inventory for a specific room
	bool hasItem(const T& i);
	// Use (remove) an item from the inventory for a specific room
	void useItem(const T& i);
	// Get the total number of items in the inventory
	int getNumItems() const;
	void clear();
	void render(sf::RenderWindow& window) const; // Render all inventory items
	Inventory();
};


