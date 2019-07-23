import argparse
from numpy import random

SEPARATOR = "+"

parser = argparse.ArgumentParser(
    description="Process a string containing dice and modifiers.")
parser.add_argument("string")

args = parser.parse_args()
rollString = args.string
rollElements = rollString.split(SEPARATOR)
print("Rolling " + rollString + ": ")


def rollDie(die):
    numberOfDice, numberOfFaces = die.split("d")
    numberOfDice = int(numberOfDice)
    numberOfFaces = int(numberOfFaces)

    ac = 0
    for i in range(numberOfDice):
        ac += random.randint(1, numberOfFaces)

    return ac


result = 0

for e in rollElements:
    if "d" in e:
        result += rollDie(e)
    else:
        result += int(e)

print(result)
