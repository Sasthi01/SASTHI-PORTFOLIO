#pragma once
#include <SFML/Audio.hpp>
#include <string>
#include <unordered_map>
#include <filesystem>
#include <iostream>
using namespace std;

enum class SoundEffects {
	UIButtons,
	StageOneSounds,
	StageTwoSounds,
	StageThreeSounds,
	Killer,
	InjuredSounds,
	DeathSounds,
	Doors
};

enum class BackgroundAudio {
	BackgroundSounds
};
class Audio
{
public:
	/* we only need one instance of the audio throughout the project, makes it simple when needing to edit sounds in different classes*/
	static Audio& getInstance();

	template<typename T>
	bool loadSound(T category);

	template<typename T>
	void playSound(T category, const string& soundName);

	void setSoundVolume(const string& soundName, float volume);
	void setMasterVolume(float volume);

	void stopSound(const string& soundName);
	void eraseSound(const string& soundName);
private:
	Audio();

	bool loadSound(const string& soundName, const string& filePath);

	template<typename T>
	string getAudioFolderPath(T category);

	struct SoundData {
		sf::SoundBuffer buffer;
		sf::Sound sound;
	};


	//we use unordered maps bcause the order for audio files doesnt matter and each audio file is unique
	unordered_map<std::string, shared_ptr<SoundData>> soundEffects;
	sf::Music backgroundMusic;

	float masterVolume = 100.0f;
	float soundVolume = 100.0f;



};
