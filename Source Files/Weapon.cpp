#include "Weapon.h"
#include <stdexcept>

Weapon::Weapon(std::string itemName, std::string imagePath, int damage)
	: Item(itemName, imagePath), damage(damage) {
}
Weapon::Weapon(string weaponName) : Item(weaponName) {
	// Default constructor
	damage = 0; // Initialize damage to 0 or any default value
}

int Weapon::getDamage() const {
	return damage;
}

