const _=require("lodash");
const db=require("../helpers/db");
const express=require("express");
const router=express.Router();

function _handshake(req, res) {
	return res.send({ status: "ok" })
	          .status(200);
}

function _login(req, res) {
	return db.getPlayer(req.params.uid)
	         .then((player) => {
		         if(player) return res.status(200)
		                              .send(player);
		         return db.createPlayer(req.params.uid, req.params.name)
		                  .then((player) => res.status(201)
		                                       .send(player)
		                  )
		                  .catch((err) => res.status(403)
		                                     .send({ err: "PLAYER NAME ALREADY EXIST" }));
	         })
	         .catch((err) => res.status(520)
	                            .send(err));
}

function _saveScore(req, res) {
	return db.saveScore(req.params.score, req.params.player1, req.params.player2)
	         .then((rank) => res.status(201)
	                            .send(rank));
}

function _top5(req, res) {
	return db.top5()
	         .then((tops) => res.status(200)
	                            .send(tops));
}

function _top10(req, res) {
	return db.top10()
	         .then((tops) => res.status(200)
	                            .send(tops));
}

router.get("/", _handshake);

router.get("/login/:uid/:name", _login);

router.get("/scores/save/:player1/:player2/:score", _saveScore);

router.get("/scores/top5", _top5);

router.get("/scores/top10", _top10);

module.exports=router;
