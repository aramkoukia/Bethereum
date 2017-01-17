pragma solidity ^0.4.0;

contract RouteCoin {
    // The public key of the buyer. Reza: we need to hash this.
    address public buyer;   

    address public seller;

    // Q: is this a Wallet address? an IP address? 
    // The destination of RREQ
    address public finalDestination;  

    // The deadline when the contract will end automatically
    uint public contractStartTime;

    // The duration of the contract will end automatically
    uint public contractGracePeriod;

    // The contract prize amount. 
    // Q: will this be with Ethers or we create a coin called RouteCoin?
    uint public contractPrice;
    
    enum State { Created, Expired, Completed, Aborted }
    State public state;

    function RouteCoin(address _finalDestination, uint _contractGracePeriod, uint _contractPrice) {
        buyer = msg.sender;
        contractStartTime = now;        
        finalDestination = _finalDestination;
        contractGracePeriod = _contractGracePeriod;
        contractPrice = _contractPrice;
    }

    modifier require(bool _condition) {
        if (!_condition) throw;
        _;
    }

    modifier onlyBuyer() {
        if (msg.sender != buyer) throw;
        _;
    }

    modifier onlySeller() {
        if (msg.sender != seller) throw;
        _;
    }

    modifier expired() {
        if (now < contractStartTime + contractGracePeriod) throw;
        _;
    }

    modifier inState(State _state) {
        if (state != _state) throw;
        _;
    }

    function foundDestinationAddress()
        expired // contract must be in the Created state to be able to foundDestinationAddress
        inState(State.Created)
        payable
    {
        seller = msg.sender;
        routeFound();
    }


    function confirmPurchase()
        onlyBuyer  // only buyer can confirm the working route 
        inState(State.Created)  // contract must be in the Created state to be able to confirmPurchase
    {
        routeAccepted();
        state = State.Completed;
        if (!buyer.send(contractPrice))
            throw;
    }

    function abort()
        onlyBuyer // only buyer can abort the contract
        inState(State.Created)  // contract must be in the Created state to be able to abort
    {
        aborted();
        state = State.Aborted;
    }

    // Events
    event aborted();
    event routeFound();
    event routeAccepted();

}