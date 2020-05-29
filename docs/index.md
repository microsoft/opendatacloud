# Open Data Platform

This repository contains the source code for managing a collection of public datasets. It was originally implemented to support Microsoft Research and is now being made available as open source.

A few assumptions are made with this implementation:

- The datasets are for public consumption. It does not include functionality to restrict access to datasets.

- An acknowledgement is required from the user to note that the license for using a dataset has been agreed to.  Therefore, a user must signon to the system (using Azure B2C) in order to record this acknowledgment.

- The application makes extended use of Azure services.

## Additional Documentation