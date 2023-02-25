using Godot;
using System;
using System.Collections.Generic;
public class Main : Node2D
{
    private readonly int default_port = 4321;
    private string PlayerName { get; set; }
    private Dictionary<int, string> Players = new Dictionary<int, string>();
    private Button HostButton { get; set; }
    private Button JoinButton { get; set; }
    private Button LeaveButton { get; set; }
    private LineEdit NameText { get; set; }
	private LineEdit AddressText { get; set; }
    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("connection_failed", this, nameof(ConnectionFailed));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));

        HostButton=(Button)GetNode("MarginContainer/VBoxContainer/HostGameButton");
        HostButton.Connect("pressed",this,nameof(HostGame));

        JoinButton=(Button)GetNode("MarginContainer/VBoxContainer/JoinGameButton");
        JoinButton.Connect("pressed", this, nameof(JoinGame));

        LeaveButton=(Button)GetNode("MarginContainer/VBoxContainer/LeaveGameButton");
        LeaveButton.Connect("pressed",this,nameof(LeaveGame));
        LeaveButton.Disabled=true;

        AddressText=(LineEdit)GetNode("MarginContainer/VBoxContainer/Address");
        NameText=(LineEdit)GetNode("MarginContainer/VBoxContainer/Name");
    }


public void HostGame(){
PlayerName = ((LineEdit)GetNode("MarginContainer/VBoxContainer/Name")).Text;

		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(default_port, 32);
		GetTree().NetworkPeer = peer;
		
		GD.Print("You are now hosting.");

		SetInGame();
		StartGame();
}
public void JoinGame(){
var address = AddressText.Text;

		GD.Print($"Joining game with address {address}");

		var clientPeer = new NetworkedMultiplayerENet();
		var result = clientPeer.CreateClient(address, default_port);

		GetTree().NetworkPeer = clientPeer;
}
public void LeaveGame(){
foreach(var player in Players){
	GetNode(player.Key.ToString()).QueueFree();
}
Players.Clear();
GetNode(GetTree().GetNetworkUniqueId().ToString()).QueueFree();

Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());
((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
GetTree().NetworkPeer = null;

SetInMenu();
}

private void PlayerConnected(int id){
	PlayerName=NameText.Text;
	GD.Print($"tell other player my name is{PlayerName} ");
	RpcId(id,nameof(RegisterPlayer),PlayerName);

}
private void PlayerDisconnected(int id){
	GD.Print("jogador desconectado");
	RemovePlayer(id);
}

private void ConnectedToServer(){
	GD.Print("Successfully connected to the server");
	SetInGame();
	StartGame();
}
private void ConnectionFailed(){
	GetTree().NetworkPeer=null;
	GD.Print("Failed to connect.");

		SetInMenu();
}
private void ServerDisconnected(int id){
GD.Print($"Disconnected from the server");

		SetInMenu();
		LeaveGame();
}
[Remote]private void RegisterPlayer(string playerName)
	{
		var id = GetTree().GetRpcSenderId();

		Players.Add(id, playerName);

		GD.Print($"{playerName} added with ID {id}");

		// a player has been added spawn in the right location
		SpawnPlayer(id, playerName);
	}
[Remote]private void StartGame()
	{
		// spawn yourself
		SpawnPlayer(GetTree().GetNetworkUniqueId(), PlayerName);
	}
private void SpawnPlayer(int id, string playerName)
	{
		// load the players
		var playerScene = (PackedScene)ResourceLoader.Load("res://player.tscn");

		var playerNode = (player)playerScene.Instance();
		playerNode.Name = id.ToString();
		playerNode.SetNetworkMaster(id);

		playerNode.Position = new Vector2(500, 250);

		playerNode.SetPlayerName(GetTree().GetNetworkUniqueId() == id ? PlayerName : playerName);

		AddChild(playerNode);
	}

[Remote]private void RemovePlayer(int id)
	{
		if (Players.ContainsKey(id))
		{
			Players.Remove(id);

			GetNode(id.ToString()).QueueFree();
		}
	}

private void SetInGame(){
		HostButton.Disabled = true;
		JoinButton.Disabled = true;
		LeaveButton.Disabled = false;
		NameText.Editable = true;
		AddressText.Editable = true;
	}

private void SetInMenu()
	{
		HostButton.Disabled = false;
		JoinButton.Disabled = false;
		LeaveButton.Disabled = true;
		NameText.Editable = false;
		AddressText.Editable = false;
	}

}
