pragma solidity ^0.4.0;

contract RouteCoin {
    // Parameters of the contract. Times are either
    // absolute unix timestamps (seconds since 1970-01-01)
    // or time periods in seconds.
    
    // The public key of the buyer. Reza: we need to hash this.
    address public buyer;   

    // Q: is this a Wallet address? an IP address? 
    // The destination of RREQ
    address public finalDestination;  

    // The deadline when the contract will end automatically
    uint public startTime;

    // The duration of the contract will end automatically
    uint public contractTime;

    // The contract prize amount. 
    // Q: will this be with Ethers or we create a coin called RouteCoin?
    uint public contractPrize;

    // Current state of the contract.
    struct ContractState
    {
        // The address of the winner of the RREQ. Reza: we need to hash this
        address public winnerAddress;
        // Stores the state of the contract. 
        // 1:Open, 2:Expired 3:Finished. Other?
        uint public finalState;
        // specifies that the contract is ended so no changes is done after
        bool contractEnded;
    } 


    // Allowed withdrawals of previous bids
    mapping(address => uint) pendingReturns;



    // Events that will be fired on changes.
    event ContractEnded(address winner, uint amount);

    // The following is a so-called natspec comment,
    // recognizable by the three slashes.
    // It will be shown when the user is asked to
    // confirm a transaction.

    function RouteCoin(address _buyer, uint _contractTime, uint _contractPrize)  
    {
        buyer = _buyer;
        startTime = now;
        contractTime = _contractTime;
        contractPrize = _contractPrize;
    }

    /// Bid on the auction with the value sent
    /// together with this transaction.
    /// The value will only be refunded if the
    /// auction is not won.
    function bid() payable {
        // No arguments are necessary, all
        // information is already part of
        // the transaction. The keyword payable
        // is required for the function to
        // be able to receive Ether.
        if (now > auctionStart + biddingTime) {
            // Revert the call if the bidding
            // period is over.
            throw;
        }
        if (msg.value <= highestBid) {
            // If the bid is not higher, send the
            // money back.
            throw;
        }
        if (highestBidder != 0) {
            // Sending back the money by simply using
            // highestBidder.send(highestBid) is a security risk
            // because it can be prevented by the caller by e.g.
            // raising the call stack to 1023. It is always safer
            // to let the recipient withdraw their money themselves.
            pendingReturns[highestBidder] += highestBid;
        }
        highestBidder = msg.sender;
        highestBid = msg.value;
        HighestBidIncreased(msg.sender, msg.value);
    }

    /// Withdraw a bid that was overbid.
    function withdraw() returns (bool) {
        var amount = pendingReturns[msg.sender];
        if (amount > 0) {
            // It is important to set this to zero because the recipient
            // can call this function again as part of the receiving call
            // before `send` returns.
            pendingReturns[msg.sender] = 0;

            if (!msg.sender.send(amount)) {
                // No need to call throw here, just reset the amount owing
                pendingReturns[msg.sender] = amount;
                return false;
            }
        }
        return true;
    }

    /// End the auction and send the highest bid
    /// to the beneficiary.
    function auctionEnd() {
        // It is a good guideline to structure functions that interact
        // with other contracts (i.e. they call functions or send Ether)
        // into three phases:
        // 1. checking conditions
        // 2. performing actions (potentially changing conditions)
        // 3. interacting with other contracts
        // If these phases are mixed up, the other contract could call
        // back into the current contract and modify the state or cause
        // effects (ether payout) to be perfromed multiple times.
        // If functions called internally include interaction with external
        // contracts, they also have to be considered interaction with
        // external contracts.

        // 1. Conditions
        if (now <= auctionStart + biddingTime)
            throw; // auction did not yet end
        if (ended)
            throw; // this function has already been called

        // 2. Effects
        ended = true;
        AuctionEnded(highestBidder, highestBid);

        // 3. Interaction
        if (!beneficiary.send(highestBid))
            throw;
    }
}