This project uses Couchbase as its database which is a NoSQL Database.﻿

Guide for the Couchbase Server.
Name the Buckets, Scope, and Collections same as below!

Bucket name: HotelBookingBucket
Scope and Collections:
	_default => _default (this is default per creating a bucket, no need creating another default)
	identityScope => roles
			   	  => userRoles
				  => users
	Reservations => Bookings

Go to appsettings.json and change the following username and password with your own username and password when logging in to couchbase server.

"Couchbase": {
    "ConnectionString": "couchbase://localhost",
    "Username": "Admin", //"change with your own username"
    "Password": "admin@123", //"change with your own password"
    "BucketName": "HotelBookingBucket"


Restore Packages
Rebuild
Run!!! or Takbo!!!

@Developed by YURI
