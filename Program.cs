class Program
{
    static Random random = new Random();

    static void Main(string[] args)
    {
        int numberOfCities = 10;
        int populationSize = 100;
        int generations = 100;
        double mutationRate = 0.02;

        List<City> cities = GenerateCities(numberOfCities).ToList();
        List<Chromosome> population = InitializePopulation(populationSize, cities).ToList();

        for (int gen = 0; gen < generations; gen++)
        {
            if (population.Count == 1) break;

            population = EvolvePopulation(population, mutationRate);
            //DisplayAllFitnesses(population);
        }

        Chromosome bestRoute = population.OrderBy(chromosome => chromosome.Fitness).First();
        Console.WriteLine("Найкраща дистанцiя маршруту: " + bestRoute.Fitness);
        Console.WriteLine("Порядок найкращого маршруту: " + GetRoute(bestRoute.Genes));

    }

    static string GetRoute(List<City> cities) => string.Join(" -> ", GetCitiesAsStrings(cities));

    static IEnumerable<string> GetCitiesAsStrings(List<City> cities)
    {
        foreach (City city in cities)
            yield return $"Мiсто {city.Index}";
    }

    static void DisplayAllFitnesses(List<Chromosome> routes)
    {
        int i = 0;

        foreach (Chromosome chromosome in routes)
        {
            Console.WriteLine($"{i}. {chromosome.Fitness}");
            i++;
        }

        Console.WriteLine();
    }

    static IEnumerable<City> GenerateCities(int numberOfCities)
    {
        for (int i = 0; i < numberOfCities; i++)
        {
            yield return new City(i, random.Next(100), random.Next(100));
        }
    }

    static IEnumerable<Chromosome> InitializePopulation(int populationSize, List<City> cities)
    {
        for (int i = 0; i < populationSize; i++)
        {
            yield return new Chromosome(cities.OrderBy(city => random.Next()).ToList());
        }
    }

    static List<Chromosome> EvolvePopulation(List<Chromosome> population, double mutationRate)
    {
        List<Chromosome> newPopulation = new List<Chromosome>();
        int elitism = (int)(population.Count * 0.1); // відосток популяції, який залишається без змін

        // збереження найкращих особини з попереднього покоління
        newPopulation.AddRange(population.OrderBy(chromosome => chromosome.Fitness).Take(elitism));

        // відбір
        int numberOfParents = population.Count - elitism;
        List<Chromosome> parents = SelectParents(population, numberOfParents);

        // схрещування і мутація
        for (int i = 0; i < parents.Count - 1; i += 2)
        {
            var children = Crossover(parents[i], parents[i + 1]);

            foreach (var ch in children)
            {
                if (random.NextDouble() > mutationRate)
                    ch.Mutate();

                newPopulation.Add(ch);
            }
        }

        return newPopulation;
    }

    static List<Chromosome> SelectParents(List<Chromosome> population, int numberOfParents)
    {
        List<Chromosome> parents = new List<Chromosome>();

        for (int i = 0; i < numberOfParents; i++)
        {
            int randomIndex;

            do randomIndex = random.Next(population.Count);
            while (parents.Contains(population[randomIndex])); // без дублікатів

            parents.Add(population[randomIndex]);
        }

        return parents;
    }

    static List<Chromosome> Crossover(Chromosome parent1, Chromosome parent2)
    {
        //Console.WriteLine(GetRoute(parent1.Genes));
        //Console.WriteLine(GetRoute(parent2.Genes));

        int crossIndex = random.Next(parent1.Genes.Count - 1);

        List<City> newParent1, newParent2;
        List<City> parent1prev, parent1next, parent2prev, parent2next;

        parent1prev = parent1.Genes.Take(crossIndex).ToList();
        parent2prev = parent2.Genes.Take(crossIndex).ToList(); parent1next = parent1.Genes.Skip(crossIndex).ToList();
        parent2next = parent2.Genes.Skip(crossIndex).ToList();

        newParent1 = parent1prev.Concat(parent2next).Concat(parent2prev).Distinct().ToList();
        newParent2 = parent2prev.Concat(parent1next).Concat(parent1prev).Distinct().ToList();

        //Console.WriteLine(GetRoute(newParent1));
        //Console.WriteLine(GetRoute(newParent2));

        return new List<Chromosome> { new Chromosome(newParent1), new Chromosome(newParent2) };
    }
}

class City
{
    public int X { get; }
    public int Y { get; }
    public int Index { get; }

    public City(int index, int x, int y)
    {
        Index = index;
        X = x;
        Y = y;
    }
}

class Chromosome
{
    public List<City> Genes { get; set; }
    public double Fitness { get; private set; }

    public Chromosome(List<City> genes)
    {
        Genes = genes;
        CalculateFitness();
    }

    public void CalculateFitness()
    {
        double distance = 0;

        for (int i = 0; i < Genes.Count - 1; i++)
            distance += CalculateDistance(Genes[i], Genes[i + 1]);

        Fitness = distance;
    }

    private double CalculateDistance(City city1, City city2) =>
        Math.Sqrt(Math.Pow(city1.X - city2.X, 2) + Math.Pow(city1.Y - city2.Y, 2));

    public void Mutate() => SwapCities(
        new Random().Next(Genes.Count),
        new Random().Next(Genes.Count));

    private void SwapCities(int index1, int index2)
    {
        City temp = Genes[index1];
        Genes[index1] = Genes[index2];
        Genes[index2] = temp;
        CalculateFitness();
    }
}