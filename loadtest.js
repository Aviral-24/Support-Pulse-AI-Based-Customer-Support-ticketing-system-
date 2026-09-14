import http from 'k6/http';
import { check, sleep } from 'k6';

// Test Configuration: Kitne users aur kitni der tak test chalana hai
export let options = {
    stages: [
        { duration: '10s', target: 50 },  // Pehle 10 second me 0 se 50 users tak badhao
        { duration: '20s', target: 50 },  // Agle 20 second tak 50 users continuously requests bhejenge
        { duration: '10s', target: 0 },   // Aakhiri 10 second me users dheere-dheere 0 kar do
    ],
};

export default function () {
    // Humare Health endpoint par barish (flood) karenge requests ki
    let res = http.get('http://localhost:5215/health');
    
    // Check karenge ki kya sabhi requests pass hui (Status 200 OK)
    check(res, { 
        'API is alive (status 200)': (r) => r.status === 200 
    });
    
    sleep(1); // Har user 1 second ka gap lega agli request se pehle
}