namespace Rmm.Crm

open System

module Domain =
    type ContactId = ContactId of Guid
    type ContactNumber = ContactNumber of string

    type Contact =
        { Id: ContactId
          Number: ContactNumber
          FullName: string }

    module Contact =
        let pretty contact =
            let (ContactNumber no) = contact.Number

            let guid =
                let (ContactId id) = contact.Id
                id.ToString()

            $"{no}: {guid} - Full Name: {contact.FullName}"

    type UserId = UserId of Guid
    type DomainAccountName = DomainAccountName of string
    type EmailAddress = EmailAddress of string option

    module EmailAddress =
        let show emailAddress =
            let (EmailAddress email) = emailAddress

            match email with
            | Some e -> e
            | None -> "None"

    type User =
        { Id: UserId
          DomainName: DomainAccountName
          FullName: string option
          PrimaryEmail: EmailAddress }

    module User =
        let pretty user =
            let guid =
                let (UserId id) = user.Id
                id.ToString()

            let (DomainAccountName account) =
                user.DomainName

            let full =
                Option.defaultValue "" user.FullName

            let email =
                (EmailAddress.show user.PrimaryEmail)

            $"%s{full} '%s{account}' email:%s{email} %s{guid}"

        let prettyOptional user =
            match user with
            | Some u -> pretty u
            | None -> "None"
