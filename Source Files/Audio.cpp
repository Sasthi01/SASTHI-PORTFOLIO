#include "Audio.h"
#include <algorithm>
#include <filesystem>
namespace fs = std::filesystem;

using namespace std;
Audio& Audio::getInstance() {
    static Audio instance;
    return instance;
}

Audio::Audio() {}

//maps an enum value from AudioCategory enum to a corresponding folder path where the needed audio files are stored

template<typename T>
string Audio::getAudioFolderPath(T category)
{
    if constexpr (is_same_v<T, SoundEffects>) {
        switch (category) {
        case SoundEffects::UIButtons: return "Assets/Audio/UIButtons/";
        case SoundEffects::StageOneSounds: return "Assets/Audio/StageOneSounds/";
        case SoundEffects::StageTwoSounds: return "Assets/Audio/StageTwoSounds/";
        case SoundEffects::StageThreeSounds: return "Assets/Audio/StageThreeSounds/";
        case SoundEffects::Killer: return "Assets/Audio/Killer/";
        case SoundEffects::InjuredSounds: return "Assets/Audio/InjuredSounds/";
        case SoundEffects::DeathSounds: return "Assets/Audio/DeathSounds/";
        case SoundEffects::Doors: return "Assets/Audio/Doors/";
        default: return "Assets/Audio/Unknown/";
        }
    }
    else if constexpr (is_same_v<T, BackgroundAudio>) {
        switch (category) {
        case BackgroundAudio::BackgroundSounds: return "Assets/Audio/BackgroundSounds/";
        default: return "Assets/Audio/Unknown/";
        }
    }
}

template<typename T>
bool Audio::loadSound(T category) {
    string folderPath = getAudioFolderPath(category);
    // checks if the folderpaths to the audio actually exists
    if (!fs::exists(folderPath)) {
        return false;
    }

    bool soundsLoaded = true;
    // the loop goes through every file in the folder and check is its a .wav file
    //directory_iterator is c++ class that aloows you to loop through folderss and files

    for (const auto& entry : fs::directory_iterator(folderPath)) {
        if (entry.path().extension() == ".wav") {
            string soundName = entry.path().stem().string(); // this will access the sound files name without the extension
            soundsLoaded &= loadSound(soundName, entry.path().string());//will return true if sound is loaded correctly
        }
    }

    return soundsLoaded;
}

template<typename T>
void Audio::playSound(T category, const string& soundName) {
    //obtains its folder path from the enum class and then adds the sound file name to it

    string soundFilePath = getAudioFolderPath(category) + soundName;
    auto it = soundEffects.find(soundName);
    if (it != soundEffects.end()) {
        it->second->sound.setVolume(masterVolume);
        if constexpr (is_same_v<T, BackgroundAudio>) {
            if (category == BackgroundAudio::BackgroundSounds) {
                it->second->sound.setLoop(true);
            }
        }
        it->second->sound.play();
    }
   
}

bool Audio::loadSound(const string& soundName, const string& filePath) {
    //if the sound already exists in the map, it returns true skipping a reload
    if (soundEffects.find(soundName) != soundEffects.end()) {
        return true;
    }

    //buffers are used to store short sounds (<1min long), its store the actual data as in their path
    auto newSound = std::make_shared<SoundData>();
    if (!newSound->buffer.loadFromFile(filePath)) {
        return false;
    }

    newSound->sound.setBuffer(newSound->buffer);
    newSound->sound.setVolume(masterVolume);
    soundEffects[soundName] = newSound;
    return true;
}
//stops the sound but it doesnt remove it from memory
void Audio::stopSound(const string& soundName) {
    auto it = soundEffects.find(soundName);
    if (it != soundEffects.end()) {
        it->second->sound.stop();
    }
   
}


//removes sound completely from memory
void Audio::eraseSound(const string& soundName) {
    auto it = soundEffects.find(soundName);
    if (it != soundEffects.end()) {
        soundEffects.erase(it);
    }
}
//used to change all of the games audio
void Audio::setMasterVolume(float volume) {
    masterVolume = std::clamp(volume, 0.0f, 100.0f);

    for (auto& [name, soundData] : soundEffects) {
        soundData->sound.setVolume(masterVolume);
    }
}
//used to change the sounds of specific audio files
void Audio::setSoundVolume(const string& soundName, float volume) {
    auto it = soundEffects.find(soundName);
    if (it != soundEffects.end()) {
        volume = max(0.f, min(100.f, volume));
        it->second->sound.setVolume(volume);
    }
}
template bool Audio::loadSound<SoundEffects>(SoundEffects);
template bool Audio::loadSound<BackgroundAudio>(BackgroundAudio);
template void Audio::playSound<SoundEffects>(SoundEffects, const string&);
template void Audio::playSound<BackgroundAudio>(BackgroundAudio, const string&);

