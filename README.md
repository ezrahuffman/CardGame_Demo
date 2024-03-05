# CardGame_Demo
 Simple online card game made in Unity. 

# Overview
The idea of this project was to use Unity's first-party solutions for multiplayer games. Most of this is known as [Unity Gaming Services](https://create.unity.com/accelerate-multiplayer?utm_source=google&utm_medium=cpc&utm_campaign=gcp_gcp_x_amer_us_en_co_sem-gg_acq_br-pr_2023-04_gcp-ga_cc3022_ev-br_id:71700000110916802&utm_content=gcp_gcp_x_amer_co_sem-gg_ev-br_pros_x_npd_cpc_kw_sd_all_x_x_opr-gcp-core_id:58700008417775585&utm_term=unity%20gaming%20services&&&&&gad=1&gclid=Cj0KCQjw7uSkBhDGARIsAMCZNJt3VYbGPE7ZZcexjlH6dF5AGZLy15PX90mTVqFgLPKB31GKiCtvwy4aAitCEALw_wcB&gclsrc=aw.ds) including Game Server Hosting, Matchmaker, and Lobby. In keeping with Unity's first-party solutions, this game also uses Netcode for Gameobjects and Cloud Storage.

# The Game
![CardGameDemo_v2_Game](https://github.com/thoroughlyswooped/CardGame_Demo/assets/35412394/2e877042-39f5-4685-afcc-8ba26d9c2b06)
Given that this is a demo project, mostly showing off the tech stack that Unity offers, the game itself is very simple. This is a simple card game where each card has a dmg and attack value. In its current iteration, only the dmg value is important. Each player selects from 14 cards at the beginning of the game until they have a full deck. Once the game starts, a card can be drawn from the deck and played to do dmg to the other player (see controls section for specifics). To end your turn you can either do every action, (i.e. draw card, use card, discard card) or you can click the skip button. This continues until someone runs out of health. As I said before, a very simple game.

# The Art
![CardGameDemo_v2_Select](https://github.com/thoroughlyswooped/CardGame_Demo/assets/35412394/aaf467c1-8e2c-4483-b5d2-8d54e310dd1b)
The art for this game was mostly created using generative AI (specifically [CatBird](https://www.catbird.ai/)). I touched up a lot of the images after but for the most part, they are pretty similar to what the AI output.

# Controls (this will be moved in-game in the next build)
* Draw Card - Left Mouse
* Play Card - Left Mouse
* Discard Card - Right Mouse

# Future Plans  
## Because this is a demo project, I don't want to spend too much time on it. I have accomplished much of what I set out to do with this project, but there are some obvious things that would make this game better. I don't know when I will return to this game, but when I do these are the next steps:
* Use health stats from cards
* SOUND (there is currently no sound for the game)
* Animations
* More sophisticated game rules. Currently, the game is very one-dimensional
* Text-chat
* AI game mode (single-player)
* Friends + Lobby (find, add friends and group into lobbies)
* Leaderboard
* More cards (Player created cards if there are ever players)

# Video Overview
https://youtu.be/d7AOWKC1Awk?si=t7L02NUH9rcq-dtl

https://github.com/thoroughlyswooped/CardGame_Demo/assets/35412394/32ff16e2-e05a-4bf5-b117-99cf72e8f9f9

