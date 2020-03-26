using Bogus.DataSets;
using Bogus.Extensions.Norway;
using Bogus.Extensions.Sweden;
using FluentAssertions;
using System;
using Xunit;

namespace Bogus.Tests.ExtensionTests
{
   public class SwedishExtensionTest : SeededTest
   {
      private void IsLegalBirthNumber(int readBirthNo, Person p)
      {
         // get last number in readBirthNo
         var genderNo = readBirthNo % 10;

         // Check odd/even individual number given gender.
         if (p.Gender == Name.Gender.Female)
         {
            (genderNo % 2 == 0).Should().BeTrue();
         }
         else
         {
            (genderNo % 2 == 0).Should().BeFalse();
         }
      }

      private void IsLegalChecksum(string readPersonnummer)
      {
         string readCs = readPersonnummer.Substring(12, 1);

         // not reading the century
         int y2 = int.Parse(readPersonnummer.Substring(2, 1));
         int y3 = int.Parse(readPersonnummer.Substring(3, 1));
         int m1 = int.Parse(readPersonnummer.Substring(4, 1));
         int m2 = int.Parse(readPersonnummer.Substring(5, 1));
         int d1 = int.Parse(readPersonnummer.Substring(6, 1));
         int d2 = int.Parse(readPersonnummer.Substring(7, 1));
         int b1 = int.Parse(readPersonnummer.Substring(9, 1));
         int b2 = int.Parse(readPersonnummer.Substring(10, 1));
         int b3 = int.Parse(readPersonnummer.Substring(11, 1));

         const int e = 2;
         const int o = 1;
         int x = grth9sum(y2 * e) + grth9sum(y3 * o) + grth9sum(m1 * e) + grth9sum(m2 * o) + grth9sum(d1 * e) + grth9sum(d2 * o) + grth9sum(b1 * e) + grth9sum(b2 * o) + grth9sum(b3 * e);
         var checksum = (Math.Ceiling(x / 10.0) * 10) - x;

         $"{checksum}".Should().Be(readCs);
      }
      private static int grth9sum(int v)
      {
         return v > 9 ? (v / 10) + (v % 10) : v;
      }

      private void IsLegalPersonnummer(string readPersonnummer, Person p)
      {
         readPersonnummer.Should().HaveLength(13);

         string dash = readPersonnummer.Substring(8, 1);
         if(DateTime.Today.AddYears(-100) >= p.DateOfBirth)
         {
            dash.Should().Be("+");
         }
         else
         {
            dash.Should().Be("-");
         }
         
         int birthNo = int.Parse(readPersonnummer.Substring(9, 3));

         IsLegalBirthNumber(birthNo, p);
         IsLegalChecksum(readPersonnummer);
      }

      [Fact]
      public void can_create_swedish_personnummer()
      {
         var f = new Faker("sv");
         var person = f.Person;
         
         string personnummer = person.Personnummer();

         IsLegalPersonnummer(personnummer, person);
      }

      [Fact]
      public void can_create_correct_checksum_1()
      {
         IsLegalChecksum("19630804-5621");
      }

      [Fact]
      public void can_verify_dash_is_plus_for_centenarian()
      {
         var f = new Faker("sv");
         var person = f.Person;
         
         person.DateOfBirth = DateTime.Now.AddYears(-100).AddDays(-1);

         string personnummer = person.Personnummer();

         IsLegalPersonnummer(personnummer, person);
      }

      [Fact]
      public void can_verify_dash_is_minus_for_noncentenarian()
      {
         var f = new Faker("sv");
         var person = f.Person;

         person.DateOfBirth = DateTime.Parse("2012-01-06 12:00 AM");

         string personnummer = person.Personnummer();

         IsLegalPersonnummer(personnummer, person);
      }
   }
}
