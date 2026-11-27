using System.Runtime.CompilerServices;

// Donne a l'assembly des tests l'acces aux membres "internal" du Core
// (ex. le constructeur de DistanceMap). Le nom doit être exactement celui de l'asmdef des tests.
[assembly: InternalsVisibleTo("FutharksAdventure.Tests")]