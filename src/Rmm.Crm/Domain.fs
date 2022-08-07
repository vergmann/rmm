namespace Rmm.Crm

open System

module Domain =
    type ContactId = ContactId of Guid
    type ContactNumber = ContactNumber of string
    
    type Contact = {
        Id : ContactId
        Number : ContactNumber
        FullName : string
    }
    
    module Contact =
        let pretty contact =
            let (ContactNumber no) = contact.Number
            let guid =
                let (ContactId id) = contact.Id
                id.ToString()
            $"{no}: {guid} - Full Name: {contact.FullName}"
