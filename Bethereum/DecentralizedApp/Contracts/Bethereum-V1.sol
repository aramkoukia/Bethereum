contract RouteCoin {
    // Parameters of the contract. Times are either
    // absolute unix timestamps (seconds since 1970-01-01)
    // or time periods in seconds.
    
    // The public key of the buyer. Reza: we need to hash this.
    address buyer;   

    // Q: is this a Wallet address? an IP address? 
    // The destination of RREQ
    address finalDestination;  

    // The deadline when the contract will end automatically
    uint contractStartTime;

    // The duration of the contract will end automatically
    uint contractGracePeriod;

    // The contract prize amount. 
    // Q: will this be with Ethers or we create a coin called RouteCoin?
    uint contractPrize;

    // Current state of the contract.
    struct ContractState
    {
        // The address of the winner of the RREQ. Reza: we need to hash this
        address winnerAddress;
        // Stores the state of the contract. 
        // 1:Open, 2:Expired 3:Finished. Other?
        uint finalState;
        // specifies that the contract is ended so no changes is done after
        bool contractEnded;
    } 

    // Events that will be fired on changes.
    //event ContractEnded(address winner, uint amount);

    function RouteCoin(address _buyer, address _finalDestination, uint _contractTime, uint _contractGracePeriod, uint _contractPrize)  
    {
        buyer = _buyer;
        finalDestination = _finalDestination;
        contractStartTime = now;
        contractGracePeriod = _contractGracePeriod;
        contractPrize = _contractPrize;
    }

    function SendRreq() {
        // No arguments are necessary, all information is already part of the transaction. 
        if (now > contractStartTime + contractGracePeriod) {
            // Revert the call if the contract grace period is over.
            throw;
        }
    }

    /// End the auction and send the highest bid
    /// to the beneficiary.
    function ContractClosed() {

        if (now <= auctionStart + biddingTime)
            throw; // auction did not yet end
        if (ended)
            throw; // this function has already been called

        // 2. Effects
        ContractState.contractEnd = true;
        //ContractEnded(winner, amount);

        // 3. Interaction
        if (!beneficiary.send(highestBid))
            throw;
    }
}