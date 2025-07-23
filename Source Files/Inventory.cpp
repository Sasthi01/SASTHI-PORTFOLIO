#include "Inventory.h"
#include "Item.h"


using namespace std;
// Add an item to the inventory for a specific room
template <typename T>
bool Inventory<T>::addItem(const T& i) {

	if (numItems >= CAPACITY) {
		return false;
	}

	if (!hasItem(i)) {
		items.push_back(i);
		numItems++;//increase total number of items in the inventory
		return true;
	}

}
// Check if an item exists in the inventory for a specific room
template <typename T>
bool Inventory<T>::hasItem(const T& i) {
	return find(items.begin(), items.end(), i) != items.end();
}
// Use (remove) an item from the inventory for a specific room
template <typename T>
void Inventory<T>::useItem(const T& i) {

	auto itemIt = std::find(items.begin(), items.end(), i);
	if (itemIt != items.end()) {
		items.erase(itemIt);//if found remove from vector
		numItems--;
	}

}
template <typename T>
void Inventory<T>::render(sf::RenderWindow& window) const {

	if (numItems == CAPACITY) {
		window.draw(fullText);
	}
	float yOffset = 10.f; // Initial vertical offset for the first item
	for (const auto& item : items) {
		sf::RectangleShape itemDisplay;
		itemDisplay.setSize({ 75.f, 75.f }); // Set a fixed size for each item
		itemDisplay.setPosition({ 25.f, yOffset }); // Position on the left side
		yOffset += 70.f; // Increment vertical offset for the next item

		// Load the item's texture
		sf::Texture* texture = new sf::Texture();
		if (texture->loadFromFile("Assets/Items/" + item.getName() + ".png")) { //  `getName` returns the texture path
			itemDisplay.setTexture(texture);
		}

		// Draw the item
		window.draw(itemDisplay);
		delete texture;
	}
}
// Explicit instantiation for Item
template class Inventory<Item>;

// Get the total number of items in the inventory
template <typename T>
int Inventory<T>::getNumItems() const {
	return numItems;

}
template <typename T>
void Inventory<T>::clear() {
	items.clear();
	numItems = 0;
}

template <typename T>
Inventory<T>::Inventory() {
	fullFont.loadFromFile("Assets/Fonts/roboto/Roboto-Regular.ttf");
	fullText.setFont(fullFont);
	fullText.setString("is full!");
	fullText.setCharacterSize(25);
	fullText.setFillColor(sf::Color::White);
	fullText.setPosition({ 40.f, 400.f });
}
