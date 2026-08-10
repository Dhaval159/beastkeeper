# BEAST KEEPER

## Master Game Design & Visual Bible

### Version 1.0 — Foundation

---

# 00. CORE IDENTITY

## Working Title

**BEAST KEEPER**

## Genre

Top-down 2D creature-collecting RPG / adventure.

Primary inspirations:

* Classic monster-collecting RPG structure
* Story-driven adventure RPGs
* Modern indie pixel-art games
* Exploration-focused RPGs
* Creature-bonding games

Beast Keeper must NOT feel like a Pokémon clone.

It may borrow the accessibility of:

> Explore → Meet creatures → Battle → Progress → Unlock areas

but its world, story, creatures, visual identity, progression, and gameplay systems must be original.

---

# 01. THE CORE FANTASY

The player is not simply a monster trainer.

The player is a:

# BEAST KEEPER

A person who learns to understand, study, bond with, and eventually fight alongside creatures known as Beasts.

The central fantasy is:

> **Explore an unfamiliar world, discover strange creatures, understand them, earn their trust, and uncover why the relationship between humans and Beasts is changing.**

The player should gradually feel:

```text
Curiosity
    ↓
Discovery
    ↓
Understanding
    ↓
Bond
    ↓
Responsibility
    ↓
Mastery
```

The player should NOT begin as an unstoppable hero.

They should begin as an inexperienced Keeper.

---

# 02. DESIGN PILLARS

Every feature should reinforce at least one of these pillars.

## Pillar 1 — Discovery

The world should constantly give the player reasons to investigate.

Examples:

* Strange footprints
* Hidden paths
* Unusual plants
* Strange sounds
* Missing villagers
* Unknown creatures
* Ancient ruins
* Environmental clues
* NPC rumors

---

## Pillar 2 — Bond

Beasts are not disposable combat units.

Each creature should feel like an individual.

Eventually the player should care about:

* Which Beast they found
* Where they found it
* How they met
* How it behaves
* What abilities it learned
* How their relationship developed

---

## Pillar 3 — Story

The game follows one long-term storyline.

The story is delivered through a sequence of smaller adventures.

Structure:

```text
MAIN STORY
│
├── Chapter 1
│   ├── Quest
│   ├── Quest
│   └── Quest
│
├── Chapter 2
│   ├── Quest
│   ├── Quest
│   └── Quest
│
├── Chapter 3
│   └── ...
```

Every short-term quest should either:

* advance the main mystery,
* develop the world,
* develop a character,
* introduce a Beast,
* reveal history,
* or unlock future possibilities.

Avoid meaningless filler quests.

---

## Pillar 4 — Exploration

The world should reward curiosity.

The player should frequently think:

> "What's over there?"

rather than:

> "Which quest marker do I follow?"

---

## Pillar 5 — Consequence

The world should react to the player's progression.

Villages can change.

NPC dialogue can change.

Areas can become accessible.

Beasts can appear/disappear.

Story events can alter locations.

The world should feel persistent.

---

# 03. VISUAL IDENTITY

## The Goal

Beast Keeper is:

# MODERN HIGH-FIDELITY PIXEL ART

Not retro imitation.

Not low-resolution nostalgia.

Not HD art with a pixel filter.

The game should feel like:

> **Classic pixel-art RPG fundamentals rendered with modern artistic sophistication.**

---

# 04. THE "MINECRAFT SHADERS" PRINCIPLE

This is the primary visual analogy.

Classic GBA RPG:

```text
Simple pixel shapes
Flat colors
Limited lighting
Simple shadows
Limited environmental detail
```

Beast Keeper:

```text
Detailed pixel shapes
Rich color relationships
Layered lighting
Soft atmospheric effects
Environmental depth
Animated details
Weather
Reflections
Glow
Subtle particles
Rich shadows
```

However:

## IMPORTANT

We must NEVER achieve this by simply adding modern effects on top of low-quality pixel art.

The underlying pixel art itself must be high quality.

Think:

> **Premium pixel art first. Modern rendering second.**

---

# 05. PIXEL ART PHILOSOPHY

Pixel art is not merely a resolution.

It is the visual language of the game.

Every pixel should appear intentional.

Avoid:

* Random noisy pixels
* Excessive dithering
* Artificial pixel filters
* AI-generated texture noise
* Inconsistent pixel sizes
* Mixed resolutions
* Smooth vector shapes disguised as pixel art

Use:

* deliberate clusters
* readable silhouettes
* controlled palettes
* strong shape language
* selective highlights
* purposeful texture

---

# 06. PIXEL DENSITY

Primary world grid:

## 32 × 32 pixels per tile

This is the foundation.

Characters should generally occupy approximately:

```text
Width: 20–32 px
Height: 32–48 px
```

depending on character design.

Small Beasts:

```text
32–48 px
```

Medium Beasts:

```text
48–64 px
```

Large Beasts:

```text
64–96+ px
```

Battle sprites may be substantially larger than overworld sprites.

The same creature must nevertheless maintain recognizable proportions between overworld and battle presentation.

---

# 07. PIXEL SCALE RULE

The project must maintain a consistent pixel scale.

Do NOT mix:

```text
8 px pixels
16 px pixels
32 px pixels
```

randomly.

All assets must be designed around the same fundamental pixel density.

---

# 08. COLOR PHILOSOPHY

Beast Keeper should use **rich but controlled palettes**.

Avoid:

* oversaturated rainbow colors everywhere
* flat neon colors
* excessive black
* pure white highlights
* muddy environments

Use:

* warm/cool contrast
* environmental color grading
* selective saturation
* atmospheric color
* strong focal colors

---

# 09. LIGHTING

Lighting is one of the biggest differences between classic and modern pixel art.

The game should use layered lighting.

Example:

```text
BASE PIXEL ART
      +
GLOBAL LIGHT
      +
LOCAL LIGHT
      +
AMBIENT SHADOW
      +
ATMOSPHERE
```

Examples:

### Village — daytime

Warm sunlight.

Soft shadows.

Bright grass.

Warm wood.

Blue sky influence.

### Forest

Cooler ambient lighting.

Dense shadow.

Filtered sunlight through trees.

Subtle green atmospheric tint.

### Ancient ruins

Cool stone.

Deep shadows.

Small supernatural light sources.

### Night

Deep blue environment.

Warm windows.

Moonlight.

Creature glow.

---

# 10. SHADOW STYLE

Shadows should be:

* readable
* soft enough to create depth
* pixel-art consistent
* directionally consistent

Characters should have small grounded shadows.

Large objects should cast stronger shadows.

Trees should create localized areas of darkness.

---

# 11. ATMOSPHERE

Modern quality should come partly from subtle environmental effects.

Examples:

* floating dust
* falling leaves
* fireflies
* rain
* mist
* water ripples
* glowing particles
* drifting pollen
* light shafts
* subtle screen-space color effects

These effects should be **subtle**.

Never turn the game into a particle showcase.

---

# 12. CAMERA

Perspective:

## Top-down 2D

Orthographic camera.

The camera should make the player feel small relative to the world.

The world should be readable while still having enough visual detail.

Avoid excessive zoom.

The player should be able to comfortably understand:

* nearby paths
* NPCs
* obstacles
* interactables
* environmental landmarks

---

# 13. WORLD STRUCTURE

The world is organized into:

```text
REGION
│
├── Village
├── Wilderness
├── Forest
├── Ruins
├── Caves
├── Mountains
└── Special Areas
```

Each region should have its own:

* visual identity
* environmental storytelling
* Beast ecosystem
* music
* NPCs
* story purpose
* secrets

---

# 14. VILLAGE DESIGN

The first village is the player's home base.

It should feel:

* safe
* warm
* familiar
* alive

It should contain:

* Keeper's house
* General shop
* NPC homes
* central gathering area
* paths
* vegetation
* signs
* small environmental stories

The village should not feel perfectly symmetrical.

Avoid:

```text
House House House
    Path
House House House
```

Instead use organic layouts.

Buildings should have:

* different orientations
* different sizes
* small yards
* fences
* decorations
* vegetation
* personal touches

---

# 15. FOREST DESIGN

The forest is the first major wilderness.

It should transition gradually.

### Forest Edge

Friendly.

Bright.

Easy to navigate.

### Mid Forest

Denser.

More mysterious.

More Beast activity.

### Deep Forest

Darker.

More atmospheric.

Strange environmental details.

### Secret Areas

Visually distinct.

Reward exploration.

---

# 16. ENVIRONMENTAL STORYTELLING

The environment should communicate information without dialogue.

Examples:

Broken fence:

> Something large passed through.

Scratch marks:

> A Beast was nearby.

Abandoned campsite:

> Someone was here.

Ancient carving:

> The region has a history.

Destroyed trees:

> Something powerful happened.

---

# 17. PLAYER DESIGN

The protagonist should be immediately recognizable.

The character should visually communicate:

> Explorer + Keeper

rather than:

> Warrior.

Design language:

* practical clothing
* travel gear
* backpack
* journal/research equipment
* Keeper emblem
* boots
* functional accessories

Avoid giant armor.

Avoid excessive weapons.

The character's primary identity is:

## A Keeper.

---

# 18. CHARACTER SILHOUETTES

Characters must remain recognizable at small sizes.

Use:

* distinct hair shapes
* clothing silhouettes
* hats
* coats
* backpacks
* tools
* accessories

Avoid relying only on facial details.

---

# 19. NPC DESIGN

NPCs should have visual storytelling.

A character's profession/personality should be visible.

Examples:

Old Keeper:

* older silhouette
* long coat
* staff
* Keeper insignia

Shopkeeper:

* apron
* bags
* merchant accessories

Explorer:

* backpack
* field equipment

Farmer:

* work clothes
* tools

Each NPC should be recognizable even without dialogue.

---

# 20. ANIMATION PHILOSOPHY

Animation is a major part of the "modern pixel art" feeling.

Avoid static sprites wherever possible.

Characters should have:

* idle animation
* walk animation
* interaction animation
* directional movement

Beasts should additionally have:

* idle personality animations
* attack animations
* hit reactions
* victory animations
* faint/defeat animations
* special ability animations

Environmental animations:

* water
* grass
* trees
* flowers
* fire
* lights
* particles

---

# 21. PLAYER WALKING

Minimum:

## 4 directions

```text
Up
Down
Left
Right
```

Each direction should have a short looping walk animation.

Target:

## 4 frames per direction

16 walking frames total.

Idle can initially be 2–4 frames.

Animation should prioritize readability rather than excessive frames.

---

# 22. BEAST DESIGN PHILOSOPHY

Beasts are the game's biggest visual opportunity.

Every Beast must have:

1. Distinct silhouette
2. Distinct color identity
3. Distinct personality
4. Distinct habitat
5. Distinct movement style
6. Distinct combat identity

Avoid simply recoloring the same creature.

---

# 23. BEAST TYPES

Types should influence:

* abilities
* weaknesses
* environment
* appearance
* behavior

Possible initial types:

* Flame
* Water
* Earth
* Nature
* Air
* Shadow
* Light
* Frost
* Electric

Do not introduce all types immediately.

The first region should contain a small number.

---

# 24. BEAST HABITATS

Creatures should feel connected to their environments.

Examples:

Forest:

* plant creatures
* small mammals
* insects
* predators

Pond:

* aquatic creatures
* amphibians

Mountain:

* stone creatures
* flying creatures

Ruins:

* ancient creatures
* supernatural creatures

This makes the world feel like an ecosystem rather than a random monster generator.

---

# 25. BEAST PERSONALITY

A Beast should not simply be:

```text
Name
HP
Attack
Defense
```

Eventually it should also have:

* temperament
* behavior
* preferred environment
* personality traits
* bonding tendencies

This doesn't have to affect combat immediately.

---

# 26. MONSTER OVERWORLD ART

Overworld Beast sprites should be smaller and readable.

They should have:

* strong silhouette
* simple readable animation
* visible personality

Examples:

A small Beast might:

* wander
* graze
* sleep
* investigate the player

A predator might:

* patrol
* watch the player
* retreat
* become aggressive

---

# 27. BATTLE ART

Battle sprites should be significantly larger.

This is where we can show the detailed artwork.

The battle presentation should feel like:

> **The camera has moved closer to the Beast.**

Not like two tiny sprites fighting.

Use:

* large Beast sprites
* expressive animations
* impact effects
* camera movement
* particles
* hit reactions
* environmental influence

---

# 28. COMBAT SYSTEM

Combat is:

## Turn-based

Initial structure:

```text
Player
  ↓
Choose action
  ↓
Beast acts
  ↓
Enemy acts
  ↓
Resolve effects
  ↓
Next turn
```

Eventually:

```text
Attack
Ability
Defend
Item
Switch
Observe
Capture/Bond
```

The system should remain readable.

---

# 29. PLAYER PROGRESSION

Progression should not simply mean:

> Bigger numbers.

Player progression should unlock:

* new regions
* new Keeper abilities
* new equipment
* new dialogue
* new Beast opportunities
* new story chapters

---

# 30. BEAST PROGRESSION

Beasts gain experience through activity and battles.

Possible progression:

```text
Level
 ↓
Stats
 ↓
Abilities
 ↓
New abilities
 ↓
Evolution / transformation
```

Do not make every Beast follow the exact same progression pattern.

---

# 31. STORY STRUCTURE

The game uses:

## One long-term storyline.

Broken into chapters.

Each chapter contains short adventures.

Example:

```text
CHAPTER 1
The Missing Herd

Quest 1 — Strange Tracks
Quest 2 — Into the Forest
Quest 3 — The Injured Beast
Quest 4 — Something Is Wrong

CHAPTER 2
The Old Ruins

Quest 1
Quest 2
Quest 3
...
```

---

# 32. STORY PACING

Avoid:

```text
Dialogue
Quest
Battle
Dialogue
Quest
Battle
```

repeating endlessly.

Instead alternate:

```text
Exploration
Dialogue
Investigation
Discovery
Battle
Character interaction
Puzzle
Travel
Story revelation
```

---

# 33. FIRST STORY ARC

The opening should establish:

* village
* protagonist
* Old Keeper
* Beasts
* wilderness
* first mystery

Potential opening:

### The Missing Herd

Animals disappear near the village.

Villagers blame a Beast.

The player investigates.

Tracks lead into the forest.

The player discovers an injured Beast.

The Beast does not behave like a normal predator.

This creates the first mystery.

---

# 34. THE CENTRAL MYSTERY

The story should eventually reveal:

> Something has fundamentally changed in the relationship between humans and Beasts.

The cause should be connected to the world's history.

The player gradually discovers:

* forgotten civilizations
* old Keeper traditions
* ancient Beast knowledge
* unexplained changes
* hidden locations
* conflicting human beliefs

Do not reveal the full mystery early.

---

# 35. DIALOGUE STYLE

Dialogue should be:

* natural
* concise
* character-specific
* occasionally humorous
* occasionally emotional
* never unnecessarily exposition-heavy

Avoid NPCs saying:

> "As you know, our village has existed for 300 years..."

Let the player discover information naturally.

---

# 36. QUEST DESIGN

Every quest should have a purpose.

Good:

> Investigate strange tracks.

Bad:

> Collect 10 mushrooms because RPG.

Tasks should create:

* curiosity
* exploration
* character development
* story progression
* discovery

---

# 37. UI PHILOSOPHY

UI should feel like part of the world.

Avoid generic modern dashboard UI.

The UI should combine:

* pixel-art elements
* clean typography
* subtle textures
* restrained animation
* clear hierarchy

The UI must remain modern and readable.

This is another:

> "Minecraft with shaders"

principle.

Classic RPG structure + modern presentation.

---

# 38. DIALOGUE UI

Dialogue box:

* lower screen
* dark translucent background
* subtle pixel border
* speaker name
* readable text
* small continuation indicator

Character portrait can be introduced later.

---

# 39. BATTLE UI

Battle UI should prioritize:

1. Beast
2. HP
3. Turn information
4. Available actions

Avoid clutter.

The battle should visually emphasize the creature, not the menus.

---

# 40. AUDIO DIRECTION

Audio should reinforce the environment.

Village:

* birds
* wind
* distant activity
* gentle music

Forest:

* insects
* leaves
* birds
* distant Beast sounds

Deep forest:

* reduced ambient noise
* strange sounds
* subtle tension

Battle:

* strong impacts
* creature sounds
* ability-specific effects

---

# 41. MUSIC

Each major region should have its own musical identity.

Themes can evolve as the story progresses.

Village music may become:

* quieter during danger
* celebratory during festivals
* melancholic after major events

Music should participate in storytelling.

---

# 42. WEATHER

Eventually support:

* sunny
* cloudy
* rain
* storm
* fog
* night

Weather can affect:

* atmosphere
* music
* Beast appearances
* environmental visuals

Do not make weather a requirement for the first playable slice.

---

# 43. DAY / NIGHT

Eventually the world should have time-of-day variation.

Village at:

### Morning

Bright and active.

### Afternoon

Warm and busy.

### Evening

Warm lights.

### Night

Quiet.

Stars.

Lanterns.

Different Beast activity.

---

# 44. WORLD REACTIVITY

The world should change as the player progresses.

Examples:

Early:

```text
Broken bridge
```

Later:

```text
Bridge repaired
```

Early:

```text
NPC refuses to travel
```

Later:

```text
NPC opens new shop
```

Early:

```text
Forest blocked
```

Later:

```text
New region accessible
```

---

# 45. VISUAL PROGRESSION

As the story becomes more mysterious, the visual language can gradually evolve.

Early game:

```text
Bright
Warm
Natural
Friendly
```

Mid game:

```text
More contrast
Stranger environments
Ancient structures
Unusual Beasts
```

Late game:

```text
Dramatic lighting
Unusual colors
Powerful environmental effects
Ancient locations
Otherworldly areas
```

The game should visually mature alongside the story.

---

# 46. ART ASSET PIPELINE

Every production asset follows:

```text
CONCEPT
 ↓
STYLE CHECK
 ↓
PIXEL ART
 ↓
IMPORT
 ↓
UNITY TEST
 ↓
ANIMATION
 ↓
IN-GAME TEST
 ↓
APPROVAL
```

Never mass-produce assets before approving the visual direction.

---

# 47. MASTER REFERENCE RULE

The first approved:

* Player
* NPC
* Tree
* Building
* Ground tile

become the visual benchmark.

All future assets must match them.

If an asset doesn't match:

> Reject or revise it.

Do not compromise visual consistency.

---

# 48. AI ART RULE

AI-generated assets may be used as production assets only after they pass the visual consistency test.

Never directly dump unrelated AI-generated images into the game.

AI should assist production, not define the art direction.

---

# 49. WHAT WE MUST AVOID

Beast Keeper must NOT look like:

### Generic AI pixel game

Symptoms:

* random detail everywhere
* inconsistent sprites
* excessive glow
* generic fantasy buildings
* noisy textures
* inconsistent pixel density
* random palettes
* over-designed UI

---

### Pokémon clone

Avoid:

* copied creature designs
* copied UI
* copied characters
* copied locations
* copied terminology
* copied story structure beyond general RPG conventions

---

### Generic RPG

Avoid:

* meaningless fetch quests
* random loot everywhere
* generic medieval fantasy
* enormous empty maps
* stat inflation
* exposition dumps

---

# 50. DEVELOPMENT PHILOSOPHY

Build the game in vertical slices.

Do NOT build 50 systems before testing the game.

Preferred workflow:

```text
BUILD
 ↓
PLAY
 ↓
IDENTIFY PROBLEM
 ↓
FIX
 ↓
EXPAND
```

Every major milestone should produce something playable.

---

# 51. CURRENT DEVELOPMENT ORDER

## PHASE 0 — Foundation

Completed:

* Project architecture
* Assemblies
* Core systems
* Player
* Interaction
* Tilemaps
* Village
* Forest
* Camera bounds

---

## PHASE 1 — First Vertical Slice

Current:

### Dialogue system

Then:

### Player visual identity

Then:

### NPC visual identity

Then:

### First quest

Then:

### Investigation sequence

Then:

### First Beast encounter

Then:

### Turn-based battle

Then:

### First Beast bonding system

Then:

### Return to village

Then:

### Story progression

---

# 52. FIRST PLAYABLE VERTICAL SLICE

The first meaningful build should contain:

```text
VILLAGE
  │
  ├── NPCs
  │
  ├── Old Keeper
  │       │
  │       ▼
  │   First Quest
  │
  ▼
FOREST
  │
  ├── Exploration
  │
  ├── Investigation
  │
  ├── Environmental clues
  │
  ▼
FIRST BEAST
  │
  ▼
TURN-BASED BATTLE
  │
  ▼
STORY REVELATION
  │
  ▼
RETURN TO VILLAGE
```

If this is fun, the game has a foundation.

---

# 53. QUALITY BAR

Before adding major content, ask:

### Visual

Does it look like one cohesive game?

### Gameplay

Is the action fun?

### Exploration

Does the player want to investigate?

### Story

Does the player want to know what happens?

### Creatures

Does each Beast feel memorable?

### Audio

Does the world feel alive?

### UI

Does the interface feel intentional?

If the answer is no:

## Fix it before expanding.

---

# 54. THE BEAST KEEPER EXPERIENCE

The intended emotional progression:

```text
"This world looks interesting."
          ↓
"What's that?"
          ↓
"I've never seen this Beast before."
          ↓
"Why is it behaving like that?"
          ↓
"I want to understand it."
          ↓
"I need to protect it."
          ↓
"What happened to this world?"
          ↓
"I need to find out."
```

That is the emotional journey of Beast Keeper.

---

# 55. THE GOLDEN RULE

When deciding whether to add a feature, ask:

> **Does this make the player more curious about the world, more connected to its characters/Beasts, or more invested in the story?**

If not:

## Don't add it just because RPGs normally have it.

---

# FINAL VISUAL TARGET

The simplest possible description of Beast Keeper's final visual identity:

> **A modern, high-fidelity pixel-art adventure RPG that retains the clarity and charm of classic handheld RPGs while adding sophisticated lighting, atmospheric depth, expressive animation, detailed environments, rich color palettes, and cinematic presentation.**

Or, using our internal analogy:

# **GBA PIXEL ART + MODERN INDIE ART DIRECTION + "SHADERS"**

Not literally shader-heavy.

The *feeling* of shaders.

The pixels remain the foundation.

The modern rendering makes those pixels feel alive.

---

# MASTER RULE FOR ANTIGRAVITY

Antigravity should treat this document as the project's visual and design authority.

When implementing a feature:

1. Preserve the established architecture.
2. Preserve the established visual language.
3. Prefer reusable systems.
4. Do not introduce unnecessary complexity.
5. Do not generate large amounts of content without approval.
6. Do not make major design decisions silently.
7. Verify every implementation in Unity.
8. Keep assets stylistically consistent.
9. Build playable vertical slices.
10. Never sacrifice the game's identity merely for technical convenience.
