using System;

namespace NumberGuesser
{
    class Program
    {

        static int number;

        static void Main(string[] args)
        {

            Random rand = new Random();
            number = rand.Next(1 , 101);

            while(true) 
            {
                Console.WriteLine("Input a number to guess");
                int guess = Convert.ToInt32(Console.ReadLine());

                if (guess == number)
                {
                    Console.WriteLine("Congratulations, you guessed the right number! Would you like to play again ? (Y/N)");
                    String try_again = Console.ReadLine();

                    if (try_again.Equals("Y"))
                    {
                        number = rand.Next(1 , 101);
                    }else
                    {
                        break;
                    }


                }else
                {
                    String output = guess > number ? "Your guess: " + guess + " was too big" : "Your guess: " + guess + " was too small";
                    Console.WriteLine(output);
                }

            }

            Console.WriteLine("Thank you for playing!");
    
        }
    }
}