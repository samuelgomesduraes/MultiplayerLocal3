using Godot;
using System;

public class player : KinematicBody2D
{
[Export] public int Speed = 200;
[Export] public int gravidade = 100;

	private Vector2 velocity = new Vector2();

	private Label NameLabel { get; set; }

	[Puppet]public Vector2 PuppetPosition { get; set; }
	[Puppet]public Vector2 PuppetVelocity { get; set; }

	public void GetInput()
	{
		velocity = new Vector2();

		if (Input.IsActionPressed("d"))
			velocity.x += 1;

		if (Input.IsActionPressed("a"))
			velocity.x -= 1;

		if (Input.IsActionPressed("w"))
			velocity.y += -2;

		velocity = velocity * Speed;

		Rset(nameof(PuppetPosition), Position);
		Rset(nameof(PuppetVelocity), velocity);
	}

	public override void _PhysicsProcess(float delta)
	{
		if (IsNetworkMaster())
		{
			GetInput();
			velocity.y+=gravidade;

		}
		else
		{
			Position = PuppetPosition;
			velocity = PuppetVelocity;
		}
		
		velocity = MoveAndSlide(velocity);

		if (!IsNetworkMaster())
		{
			PuppetPosition = Position;
		}
	}

	public void SetPlayerName(string name)
	{
		NameLabel = (Label)GetNode("Label");

		PuppetPosition = Position;
		PuppetVelocity = velocity;

		NameLabel.Text = name;
	}
}
