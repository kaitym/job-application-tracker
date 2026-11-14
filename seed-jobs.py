#!/usr/bin/env python3
# Seed the Job Tracker with your real applications.
# Run once, from the repo root, while the API is running:
#   python3 seed-jobs.py
# It logs in, then POSTs each job in seed-jobs.json to /jobs.
# Run it only once - running it again will add duplicates.

import json
import urllib.request

BASE = "http://localhost:5181"

with open("seed-jobs.json", encoding="utf-8") as f:
    jobs = json.load(f)

# log in
login = json.dumps({"username": "kaity", "password": "dev-password"}).encode()
req = urllib.request.Request(
    f"{BASE}/auth/login",
    data=login,
    headers={"Content-Type": "application/json"},
    method="POST",
)
with urllib.request.urlopen(req) as resp:
    token = json.load(resp)["token"]

for job in jobs:
    body = json.dumps(job).encode()
    req = urllib.request.Request(
        f"{BASE}/jobs",
        data=body,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {token}",
        },
        method="POST",
    )
    with urllib.request.urlopen(req) as resp:
        created = json.load(resp)
    print(f"Added {created['id']}: {job['company']} - {job['role']}")

print(f"Done - added {len(jobs)} jobs.")
