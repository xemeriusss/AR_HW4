# Physics Rendering via Ray Casting with Black Holes

**Course:** CSE462/562 – Augmented Reality (Fall 2024)    

## Description
This project implements a ray-casting renderer with a unique twist: the inclusion of black holes that affect ray trajectories. The renderer is built using Unity and adheres to the requirements outlined in the assignment.

## Features
1. **3D World:**
   - A scene with at least 4 separate objects, collectively comprising 10,000+ triangles.
   - Objects are rendered with Lambertian materials.
   - Adjustable poses for objects.

2. **Lighting:**
   - Three distinct light sources with adjustable intensity and position.

3. **Camera:**
   - A pinhole camera model with configurable field of view (FoV), center, and viewing directions.

4. **Ray-Casting Physics:**
   - **Without Black Holes:** Rays travel in straight lines.
   - **With Black Holes:** Rays follow quadratic curves, bending towards the black hole.

5. **Rendering Output:**
   - A rendered image of size 640x480, generated using the custom ray caster.

