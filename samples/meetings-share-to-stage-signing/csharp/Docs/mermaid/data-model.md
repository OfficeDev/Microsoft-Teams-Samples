# In-Meeting Document Signing - Mermaid flow diagrams
The below is a data model describing the data flow of our backend system.

*If viewing locally, install the Visual Studio Code "[Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)" extension for the best experience*

# Backend data models
```mermaid
classDiagram
    User <|-- Viewer
    User <|-- Signature
    Document <|-- Signature
    Document <|-- Viewer

    class User{
        String userId
        String userName
    }

    class Viewer{
        User viewer
    }

    class Signature{
      Guid id
      User signer
      DateTime signedDateTime
      String text
      bool isSigned
    }

    class Document{
      Guid id 
      string documentType
      string ownerId
      DocumentState documentState
      ICollection Viewer viewers
      ICollection Signature signatures
    }
```