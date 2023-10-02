protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (BarsInProgress == 0)
            {
				Print(DeltarTurnaroud.Get_Turnaround(Count-1));
			}
			
		}
