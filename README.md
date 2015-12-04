# hwa-cli
Command-Line Interface for converting Windows Hosted Web Apps

## Usage
`hwa-cli --manifest <manifest-path> --identityName <identity-guid> --publisherIdenty <publisher-identity> --publisherDisplayName <publisher-display-name> --makeAppxPath <path-to-makeappx-utility>`

Options: 

  `-m`, `--manifest`                Required. Input file with JSON manifest to be processed

  `-i`, `--identityName`            Required. Identity GUID

  `-p`, `--publisherIdentity`       Required. Publisher Identity. e.g. "CN=author"

  `-n`, `--publisherDisplayName`    Required. Displayed name of the publisher.

  `-a`, `--makeAppxPath`            Path to the MakeAppx command.

  `-o`, `--out`                     Path to output file for errors and messages.

  `-c`, `--outputToConsole`         (Default: True) Indicates if the program should output to console.

  `-v`, `--verbose`                 (Default: False) Indicates if the program should log verbose messages.

  `-w`, `--wait`                    (Default: False) Indicates if the program should wait for user input before closing.

  `--help`                          Display this help screen.
