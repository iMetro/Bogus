using System;

namespace Bogus.Extensions.Sweden
{
   /// <summary>
   /// API extensions specific for a geographical location.
   /// </summary>
   public static class ExtensionsForSweden
   {
      /// <summary>
      /// Swedish national identity number (personnummer)
      /// </summary>
      public static string Personnummer(this Person p)
      {
         const string Key = nameof(ExtensionsForSweden) + "Personnummer";
         if (p.context.ContainsKey(Key))
         {
            return p.context[Key] as string;
         }

         /*
             YYYYMMDD-XXXC
             |   | | ||  |--> Checksum
             |   | | ||  
             |   | | ||
             |   | | ||-----> Birthnumber where 2 first are Birthplace for personnumbers issued before 1st jan 1990, and the last is gender, Even for women and Odd for men
             |   | | |------> Dash, that changes to plus(+) when 100 years are reached
             |   | |--------> Day
             |   |----------> Month
             |--------------> Year

             The birthnumber number has to be even for women and odd for men.

             The checksum is calculated with a modulo checksum algorithm.
             If either of the checksum numbers are 10, the fødselsnummer gets
             rejected, and a new individual number has to be generated.

             https://en.wikipedia.org/wiki/Personal_identity_number_(Sweden)

         */

         var r = p.Random;
         
         string birthNumber = GenerateBirthNumber(p);
         string birthDate = $"{p.DateOfBirth:yyMMdd}";
         string checksum = GenerateChecksum(birthDate, birthNumber);

         // when the age is 100 and up the dash (-) is changed to a plus (+)
         var dash = DateTime.Today.AddYears(-100) >= p.DateOfBirth ? "+" : "-";

         string final = $"{p.DateOfBirth:yyyyMMdd}{dash}{birthNumber}{checksum}";

         p.context[Key] = final;
         return final;
      }

      private static string GenerateBirthNumber(Person p)
      {
         var r = p.Random;
         
         // Birthnumber is a 3 digit number, 001 - 999. Even for women and odd for men
         int birthplaceNumber = p.Gender == DataSets.Name.Gender.Female ? r.Even(1, 999) : r.Odd(1, 999);

         return birthplaceNumber.ToString("D3");
      }

      private static string GenerateChecksum(string birthDate, string birthNumber)
      {
         int y1 = int.Parse(birthDate.Substring(0, 1));
         int y2 = int.Parse(birthDate.Substring(1, 1));
         int m1 = int.Parse(birthDate.Substring(2, 1));
         int m2 = int.Parse(birthDate.Substring(3, 1));
         int d1 = int.Parse(birthDate.Substring(4, 1));
         int d2 = int.Parse(birthDate.Substring(5, 1));
         int b1 = int.Parse(birthNumber.Substring(0, 1));
         int b2 = int.Parse(birthNumber.Substring(1, 1));
         int b3 = int.Parse(birthNumber.Substring(2, 1));
         const int e = 2;
         const int o = 1;
         int x = SumIndividualDigitsInANumberGreaterThan9(y1 * e) + SumIndividualDigitsInANumberGreaterThan9(y2 * o) + SumIndividualDigitsInANumberGreaterThan9(m1 * e) + SumIndividualDigitsInANumberGreaterThan9(m2 * o) + SumIndividualDigitsInANumberGreaterThan9(d1 * e) + SumIndividualDigitsInANumberGreaterThan9(d2 * o) + SumIndividualDigitsInANumberGreaterThan9(b1 * e) + SumIndividualDigitsInANumberGreaterThan9(b2 * o) + SumIndividualDigitsInANumberGreaterThan9(b3 * e);
         var checksum = (Math.Ceiling(x / 10.0) * 10) - x;

         return $"{checksum}";
      }

      /// <summary>
      /// Sum individual digits if larger than 9, e.g. 14 => 1 + 4 = 5
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      private static int SumIndividualDigitsInANumberGreaterThan9(int x)
      {
         return x > 9 ? (x / 10) + (x % 10) : x;
      }
   }
}
