pragma solidity ^0.4.0;

contract Payout {
     address Victor;
     address Jim;
     address Kieren;

     mapping (address => uint) ownershipDistribution; 

     function Setup() {
       Victor = 0xaabb;
       Jim    = 0xccdd;
       Kieren = 0xeeff;


       ownershipDistribution[Victor] = 35;
       ownershipDistribution[Jim]  = 35;
       ownershipDistribution[Kieren] = 30;
     }

     function Dividend() {
       uint bal= this.balance;
       if(!Victor.send(bal * ownershipDistribution[Victor] / 100))
          throw;
       if(!Jim.send(bal * ownershipDistribution[Jim] / 100))
          throw;
       if(!Kieren.send(bal * ownershipDistribution[Kieren] / 100))
          throw;
     }
}
