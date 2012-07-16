Newsbin Remote 2.0.0.0
July 16, 2012

Completely new code. Includes the Newsbin Remote Library, a new library encapsulating the nitty gritty
remote interface implementation details in a NewsbinClient class. Also includes a test client that can 
issue any call supported by the library.
 
The Newsbin Remote Library can be used in any .NET application, including desktop and web projects.

The solution is a VS2010 solution using .NET 3.5.

The file Newsbin.Remote.zip contains all source code, with a compiled version of the test client
located in the Newsbin.Remote.TestClient/bin/Release folder.

The file nbremote.rar contains the original deprecated code.

To connect with the test client,

1) Enter the IP address of the machine Newsbin is running on
2) Enter the port that Newsbin will be listening on (default is 118)
4) Enter the password (Set under Options/Remote Control within Newsbin)
5) Click the "Connect" button

Status:

- Files tab File Download is not hooked up.
- NZB file upload is not hooked up.

Change Log:

7/17/12 2.0.0.0.b2

- Changed Response.Status to an Enum.
- Implemented NzbUpload and FilesDownload commands.
- Fixed a problem where a UTF8 BOM was sent to Newsbin in the first packet.

7/16/12 2.0.0.0.b1

- Added new Client library.
- Added new TestClient app.

9/12/11 1.0.0.1

- Added tool tips
- Added program name and version to title bar
- Fixed bug with Upload Files

9/11/11 1.0.0.0

- Initial checkin of kindof working code.