using Microsoft.EntityFrameworkCore;
using Phonebook.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Phonebook.Controllers;

internal class PhonebookController
{
    internal static void AddContact(Contact contact)
    {
        using var db = new PhonebookContext();
        db.Add(contact);
        db.SaveChanges();
    }
    internal static List<Contact> GetContacts()
    {
        using var db = new PhonebookContext();
        var contacts = db.Contacts
            .Include(x => x.Category)
            .ToList();
        return contacts;
    }
    internal static Contact GetContactById(int id)
    {
        using var db = new PhonebookContext();
        var contact = db.Contacts
            .Include(x => x.Category)
            .SingleOrDefault(x => x.ContactId == id);
        return contact;
    }
    internal static void DeleteContact(Contact contact)
    {
        using var db = new PhonebookContext();
        db.Remove(contact);
        db.SaveChanges();
    }
    internal static void UpdateContact(Contact contact)
    {
        var updatedContact = new Contact();
        updatedContact.ContactId = contact.ContactId;
        updatedContact.CategoryId = contact.CategoryId;
        updatedContact.Name = contact.Name;
        updatedContact.PhoneNumber = contact.PhoneNumber;
        updatedContact.EmailAddress = contact.EmailAddress;

        using var db = new PhonebookContext();
        db.Update(updatedContact);
        db.SaveChanges();
    }
    internal static void SendSMS(Contact contact)
    {
        Console.WriteLine("Enter the SMS text: ");
        string body = Console.ReadLine();

        var accountSid = "AC0761fbc9ed4c6da7129dc69aa95800c9";
        var authToken = Environment.GetEnvironmentVariable("Twilio_SMS_Token");
        TwilioClient.Init(accountSid, authToken);

        var messageOptions = new CreateMessageOptions(
          new PhoneNumber("+32" + contact.PhoneNumber));
        messageOptions.From = new PhoneNumber("+12568240619");
        messageOptions.Body = body;

        try
        {
            MessageResource.Create(messageOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Oops something went wrong, " + ex.Message);
            Console.ReadKey();
        }

    }
    internal static async Task SendEmail(Contact contact)
    {
        Console.WriteLine("Enter the subject for the email: ");
        string subject = Console.ReadLine();
        Console.WriteLine("Enter the text for the email: ");
        string body = Console.ReadLine();
        string toEmail = contact.EmailAddress;

        string apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Sendgrid API key not found in environment variable.");
        }
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("carlmalfliet@proximus.com", "Carl's Phonebook app");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: body);
        await client.SendEmailAsync(msg);
    }
}