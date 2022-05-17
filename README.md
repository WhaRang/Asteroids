# Asteroids

*This project introduces particle self-collision mechanism using job system and Unity's in-build particle system.
The game itself is simple old-style asteroid game the essence of which is to live as long as possible on a spaceship under a hail of asteroids.*

## Rules
* The map is endless.
* Ship is destroying when coliding with asteroid.
* All simulation is held while the game is lost.
* Steering: W or Arrow up - Forward; A, D or Arrow left, right - Rotation. 
* Ship automatically shoots projectiles every 0.5 seconds.
* Projectiles are destroyed after they existed for 3 seconds or after a collision with the
asteroid.
* When projectile destroys the asteroid, a score is increased and Text on UI with score changes.
* Asteroids are destroyed after a collision with other asteroids or with the bullet.
* Destroyed asteroids are re-spawned after 1 second at random position that is
outside of the player view frustum (asteroid can spawn on another asteroid causing immediate
collision).
* Asteroids are spawning on the grid of predifined size, one asteroid in the center of one grid tile.
* Ship spawns at the center of the map.
* All asteroids have random speed larger than 0 and random direction.
* Every asteroid speed and direction is persistent every game.
* Asteroids are always simulated (flying, colliding and rendered) even if they are not visible.

## Particle self-collision system
Job scheduling is done in a new `MonoBehaviour` callback: `OnParticleUpdateJobScheduled`. 
This callback occurs after the native built-in particle jobs are scheduled and scheduling your jobs here ensures that the managed job runs after the built-in update.
It can return a JobHandle that reflects this job-chain.

## Performance
This project was tested on Gigabyte Aorus 15P laptop.

Device specs:
* System: Windows x64
* Processor: Intel Core i7-11800H
* Chipset: Intel HM570
* Graphics: NVIDIA GeForce RTX 3060
* RAM: 16 GB (DDR4, 3200MHz)

160x160 grid:
* 25600 particles
* 160 FPS, 6.3 ms per frame 

256x256 grid:
* 65536 particles
* 60 FPS, 16.7 ms per frame

512x512 grid:
* 262144 particles
* 30 FPS, 33.3 ms per frame

## Screenshots

### Game view
![This is a game view](/Asteroids/Screenshots/Screenshot_Game.png "Game view")

### Scene view with 25600 asteroids rendered
![This is a scene view with 25600 asteroids rendered](/Asteroids/Screenshots/Screenshot_Scene.png "Scene view")
