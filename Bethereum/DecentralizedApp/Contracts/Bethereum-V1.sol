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
    uint public contractPrize;
    
    enum State { Created, Expired, Completed, Aborted }
    State public state;

    //function Purchase() payable {
    //    seller = msg.sender;
    //value = msg.contractPrize / 2;  // transfer half of fee
    //if (2 * value != msg.value) throw;
    //}

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

    modifier inState(State _state) {
        if (state != _state) throw;
        _;
    }

    event expired();
    event aborted();
    event routeFound();
    event routeAccepted();

    function abort()
        onlyBuyer // only buyer can abort the contract
        inState(State.Created)
    {
        aborted();
        state = State.Aborted;
        //if (!buyer.send(this.balance))
        //    throw;
    }

    /// Confirm the purchase as buyer.
    /// Transaction has to include `2 * value` ether.
    /// The ether will be locked until confirmReceived
    /// is called.
    function foundDestinationAddress()
        onlySeller
        inState(State.Created)
        payable
    {
        routeFound();
        buyer = msg.sender;
        //state = State.Locked;
    }

    /// Confirm that you (the buyer) received the item.
    /// This will release the locked ether.
    function confirmPurchase()
        onlyBuyer
        inState(State.Created)
    {
        routeAccepted();
        state = State.Completed;
        if (!buyer.send(contractPrize))
            throw;
    }
}