using UnityEngine;

public class Door : AInteractable
{
    private Animator anm;
    private bool isOpen;

    protected override void Start()
    {
        base.Start();
        anm=GetComponent<Animator>();
    }

    public override void Interact()
    {
        isOpen = !isOpen;
        anm.SetBool("Open",isOpen);
    }
}
