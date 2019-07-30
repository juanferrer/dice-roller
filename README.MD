# dice-roller

A C# dice roller based on [GreenImp](https://github.com/GreenImp)'s work on [rpg-dice-roller](https://github.com/GreenImp/rpg-dice-roller). It accepts dice notation.

## Supported notation

### Standard
```
d12+2: [8]+2 = 10
4d6-L: [4,3,1,3]-L = 10
d% : [48] = 48              // Same as d100
```

### Operators
```
1d6*5 [5]*5 = 25
2d10/d20: [5,7]/[5] = 2.4
(1d6+2)*3: ([4]+2)*3 = 18
```

### Exploding dice
```
2d6!: [4, 6!, 6!, 2] = 18
1d6!-L: [6!,6!,6!,3]-L = 18
```

#### Penetrating

```
2d6!p: [4, 6!p, 5!p, 1] = 16
```

#### Compounding

```
2d6!!: [4, 14!!] = 18   // The exploded dice rolls of [6, 6, 2] are added together
1d6!!-L: [21!!]-L = 18  // The exploded dice rolls of [6, 6, 6, 3] are added together
2d6!p: [4, 12!!p] = 16  // The exploded dice rolls of [6, 5, 1] are added together
```

#### Compare point

```
2d6!=4  // Explode only if a 4 is rolled
2d6!!>3 // Compound if you roll higher than 3
```

### Fudge dice
```
dF      // This is the same as `dF.2`
4dF.2   // Roll 4 standard fudge dice
4dF.2-L // Roll 4 standard fudge dice, subtracting the lowest result
dF.1*2  // Roll non-standard fudge dice, multiplying the result by 2
```

### Dice pools
```
4d3>1: [1,3*,2*,1] = 2          // Higher than a 1 is a success
2d6>4*d6!: [6*,5*]*[6!,4] = 20  // 2 successes * raw values of the other rolls
2d6>4+H: [3,5*]+H = 2           // Highest roll is 5, which is a success, so `H` has a value of 1
2d6<4+H: [3*,5]+H = 1           // Highest roll is 5, which is a failure, so `H` has a value of 0
```

